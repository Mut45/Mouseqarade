
using System.Collections.Generic;
using System.Data.Common;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Dispatches based on user input and stores UI state, local only,
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
    
    private List<ItemInventoryEntry> sortedAvailableItems = new();
    private ItemDefinition currentlySelectedItem;
    private ItemId currentSelectedItemId;
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

        if (useJustPressed)
        {
            HandleUseItemInput();
        }

        bool cycleJustPressed = currInput.CyclePressed && !prevInput.CyclePressed;

        if (cycleJustPressed)
        {
            HandleCycleItemInput();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CancelAOETargeting();
        }
    }

    private void HandleUseItemInput()
    {
        if (aoeTargetController != null && aoeTargetController.IsTargeting)
        {
            aoeTargetController.ConfirmTargeting(currentlySelectedItem);
            return;
        }

        TryUseSelectedItem();
    }

    private void HandleCycleItemInput()
    {
        if (sortedAvailableItems.Count == 0)
        {
            ResetSelection();
            return;
        }
        currentlySelectedIndex++;
        if (currentlySelectedIndex >= sortedAvailableItems.Count) currentlySelectedIndex = 0;
        Debug.Log("[MouesItemController] Item selection switched, currently selected item is: " + currentlySelectedItem.DisplayName);
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
                    return;
                }
            }
        }

        currentlySelectedIndex = 0;
        currentlySelectedItem = GetDefininitionFromEntry(sortedAvailableItems[0]);
    }

    private void ResetSelection()
    {
        currentlySelectedItem = null;
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

}