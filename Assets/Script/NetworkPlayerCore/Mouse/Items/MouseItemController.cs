
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dispatches based on user input, local only
/// </summary>
[RequireComponent(typeof(ItemUseController))]
[RequireComponent(typeof(AOETargetingController))]
[RequireComponent(typeof(PlayerItemInventory))]
public class MouseItemController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private AOETargetingController aoeTargetController;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private ItemUseController itemUseController;
    [SerializeField] private AOETargetingController targetingController;
    [SerializeField] private PlayerItemInventory inventory;
    [SerializeField] private PlayerHandUIController handUIController;
    
    private List<ItemInventoryEntry> sortedAvailableItems = new();
    private ItemDefinition currentlySelectedItem;
    private int currentlySelectedIndex;
    public bool IsAoeTargeting;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerItemInventory>();
        }

        if (itemUseController == null)
        {
            itemUseController = GetComponent<ItemUseController>();
        }

        if (aoeTargetController == null)
        {
            aoeTargetController = GetComponent<AOETargetingController>();
        }

    }

    void OnEnable()
    {
       if (inventory == null) return;

       inventory.OnInventoryChanged += HandleInventoryChanged;
        UpdateSortedAvailableItems();
        RefreshSelectionAfterInventoryChanged();
        RefreshHandUI();
    }

    void OnDisable()
    {
        if (inventory == null) return;

        inventory.OnInventoryChanged -= HandleInventoryChanged;
    }

    #region Input Handlers
    public void HandleLocalInput(PlayerInputNetworkData prevInput, PlayerInputNetworkData currInput)
    {
        bool useJustPressed = currInput.SecondaryPressed && !prevInput.SecondaryPressed;
        bool cycleJustPressed = currInput.CyclePressed && !prevInput.CyclePressed;
        bool flipJustPressed = Input.GetKeyDown(KeyCode.X);
        bool cancelJustPressed = Input.GetKeyDown(KeyCode.C);

        // Tis handles the situation where the aoe targeting flow has started
        if (aoeTargetController != null && aoeTargetController.IsTargeting)
        {
            if (useJustPressed)
            {
                aoeTargetController.ConfirmTargeting(currentlySelectedItem);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                CancelAOETargeting();
            }

            return;
        }
        if (cancelJustPressed)
        {
            HandleCancelInput();
            return;
        }

        if (useJustPressed)
        {
            HandleUseItemInput();
        }

        if (cycleJustPressed && handUIController != null && handUIController.State == PlayerHandUIState.Selecting)
        {
            HandleCycleItemInput();
        }

        if (flipJustPressed && handUIController != null &&  
            (handUIController.State == PlayerHandUIState.Selecting || handUIController.State == PlayerHandUIState.SpecificInformation)
        )
        {
            handUIController?.ToggleSelectedCardDetails();
        }


    }

    private void HandleUseItemInput()
    {
        if (handUIController == null) return;

        switch (handUIController.State)
        {
            case PlayerHandUIState.Default:
                UpdateSortedAvailableItems();
                RefreshSelectionAfterInventoryChanged();

                if (sortedAvailableItems.Count == 0)
                {
                    ResetSelection();
                    RefreshHandUI();
                    return;
                }

                ValidateSelection();
                RefreshHandUI();
                handUIController.SetState(PlayerHandUIState.Selecting);
                break;

            case PlayerHandUIState.Selecting:
                ValidateSelection();
                handUIController.SetState(PlayerHandUIState.Default);
                TryUseSelectedItem();
                break;
            
            case PlayerHandUIState.SpecificInformation:
                ValidateSelection();
                handUIController.SetState(PlayerHandUIState.Default);
                TryUseSelectedItem();
                break;
        }
    }

    private void HandleCycleItemInput()
    {
        if (sortedAvailableItems.Count == 0)
        {
            ResetSelection();
            RefreshHandUI();
            return;
        }

        currentlySelectedIndex++;
        if (currentlySelectedIndex >= sortedAvailableItems.Count) currentlySelectedIndex = 0;
        currentlySelectedItem = GetDefininitionFromEntry(sortedAvailableItems[currentlySelectedIndex]);
        //currentSelectedItemId = sortedAvailableItems[currentlySelectedIndex].ItemId;

        RefreshHandUI();
        handUIController?.SetCurrentlySelected(currentlySelectedIndex);
        Debug.Log("[MouesItemController] Item selection switched, currently selected item is: " + currentlySelectedItem.DisplayName);
    }

    private void HandleCancelInput()
    {
        if (handUIController == null) return;

        switch (handUIController.State)
        {
            case PlayerHandUIState.Selecting:
                handUIController.CancelSelection();
                break;

            case PlayerHandUIState.SpecificInformation:
                handUIController.SetState(PlayerHandUIState.Default);
                break;
        }
    }
    #endregion

    #region Inventory Change Handling
    private void HandleInventoryChanged()
    {
        // Edge case 1: Player selected item A, item A gets consumed and its count becomes 0, the selected item should then
        // becomes the next available item if there is one
        // Edge case 2: Player used up all of the available items, the selected item becomes item none with empty icon
        UpdateSortedAvailableItems();
        RefreshSelectionAfterInventoryChanged();
        RefreshHandUI();
        // handUIController?.SetItems(sortedAvailableItems);
        // handUIController?.SetSelectedIndex(currentlySelectedIndex);
    }

    private void RefreshSelectionAfterInventoryChanged()
    {
        if (sortedAvailableItems.Count == 0)
        {
            ResetSelection();
            return;
        }

        if (currentlySelectedItem != null)
        {
            for (int i = 0; i < sortedAvailableItems.Count; i++)
            {
                if (sortedAvailableItems[i].ItemId == currentlySelectedItem.Id)
                {
                    currentlySelectedIndex = i;
                    currentlySelectedItem = GetDefininitionFromEntry(sortedAvailableItems[i]);
                    //currentSelectedItemId = sortedAvailableItems[i].ItemId;
                    return;
                }
            }
        }

        currentlySelectedIndex = 0;
        currentlySelectedItem = GetDefininitionFromEntry(sortedAvailableItems[0]);
        //currentSelectedItemId = sortedAvailableItems[0].ItemId;
    }

    private void ResetSelection()
    {
        currentlySelectedItem = null;
        //currentSelectedItemId = ItemId.None;
        currentlySelectedIndex = -1;
    }
    #endregion

    #region Item UI Sorting
    private void UpdateSortedAvailableItems()
    {
        sortedAvailableItems.Clear();
        if (inventory == null) return; 
        List<ItemInventoryEntry> availableItems = inventory.GetOwnedItemEntries();
        
        availableItems.Sort(ItemEntryComparator);
        for (int i = 0; i < availableItems.Count; i++)
        {
            sortedAvailableItems.Add(availableItems[i]);
        }

    }

    private int ItemEntryComparator(ItemInventoryEntry a, ItemInventoryEntry b)
    {
        int orderA = GetCycleOrder(a.ItemId);
        int orderB = GetCycleOrder(b.ItemId);
        
        int result = orderA.CompareTo(orderB);
        if (result != 0) return result;

        return 0;
    }
    #endregion

    #region Dispatching item use flow based on item use mode
    private void TryUseSelectedItem()
    {
        if (currentlySelectedItem == null) return;

        switch (currentlySelectedItem.UseMode)
        {
            case ItemUseMode.Instant:
                UseInstantItem(currentlySelectedItem);
                break;
            case ItemUseMode.AOETargeted:
                StartAOETargeting(currentlySelectedItem);
                break;
        }
    }

    private void UseInstantItem(ItemDefinition item)
    {
        if (itemUseController == null) return;
        ItemUseNetworkRequestData request = new ItemUseNetworkRequestData
        {
          ItemId = item.Id,
          HasTargetPosition = false,
          TargetPosition = Vector2.zero,
        };
 
        itemUseController.RequestUseItem(request);
    }

    #endregion

    #region AOE Targeting Flow
    private void StartAOETargeting(ItemDefinition item)
    {
        if (aoeTargetController == null) return;

        if (item.UseMode != ItemUseMode.AOETargeted) return;

        int itemLevel = inventory.GetItemLevel(item.Id);
        if (itemLevel >= 0)
        {
            aoeTargetController.StartTargeting(item, itemLevel);
        }

    }

    private void CancelAOETargeting()
    {
        if (aoeTargetController == null) return;

        if (aoeTargetController.IsTargeting) aoeTargetController.CancelTargeting();
    }

    #endregion

    #region Card Hand UI Visuals
    public void SetHandUIController(PlayerHandUIController handUI)
    {
        if (handUI != null)
        {
            handUIController = handUI;
        }
        
    }

    private void RefreshHandUI()
    {
        Debug.Log("[MouseItemController] RefreshHandUI called.");

        if (handUIController == null) 
        {
            Debug.LogWarning("[MouseItemController] handUIController is null.");

            return;
        }
        
        Debug.Log("[MouseItemController] Binding hand UI. itemCount=" + sortedAvailableItems.Count);
        Debug.Log("[MouseItemController] itemDatabase is " + (itemDatabase != null ? "set" : "null"));
        handUIController.BindCards(sortedAvailableItems, itemDatabase, currentlySelectedIndex);
    }
    #endregion

    #region Helper Funcitons
    private int GetCycleOrder(ItemId itemId)
    {
        if (itemDatabase == null)
            return 0;

        if (!itemDatabase.TryGetDefinition(itemId, out ItemDefinition definition))
            return 0;

        return definition.CycleOrder;
    }

    private ItemDefinition GetDefininitionFromEntry(ItemInventoryEntry entry)
    {
        if (itemDatabase == null) return null;
        if (itemDatabase.TryGetDefinition(entry.ItemId, out ItemDefinition definition))
        {
            return definition;
        }

        return null;
    }
    #endregion

    #region Validation
    private void ValidateSelection()
    {
        if (sortedAvailableItems.Count == 0)
        {
            ResetSelection();
            return;
        }

        if (currentlySelectedIndex < 0 || currentlySelectedIndex >= sortedAvailableItems.Count)
        {
            currentlySelectedIndex = 0;
        }

        ItemInventoryEntry entry = sortedAvailableItems[currentlySelectedIndex];
        currentlySelectedItem = GetDefininitionFromEntry(entry);
        //currentSelectedItemId = entry.ItemId;
    }
    #endregion

}
