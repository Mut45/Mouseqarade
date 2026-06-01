using Unity.Netcode;
using UnityEngine;

public class SandboxItemInventoryTester : NetworkBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerItemInventory targetInventory;

    [Header("Debug Add Settings")]
    [SerializeField] private ItemId itemToAdd = ItemId.SmokeBomb;
    [SerializeField] private int amountToAdd = 3;
    [SerializeField] private int levelToSet = 1;

    private void Update()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RequestAddItem(ItemId.SmokeBomb, amountToAdd);
        }


        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RequestAddItem(itemToAdd, amountToAdd);
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            RequestSetItemLevel(itemToAdd, levelToSet);
        }
    }

    private void RequestAddItem(ItemId itemId, int amount)
    {
        if (IsServer)
        {
            AddItemFromServer(itemId, amount);
        }
        else
        {
            RequestAddItemServerRpc(itemId, amount);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestAddItemServerRpc(ItemId itemId, int amount)
    {
        AddItemFromServer(itemId, amount);
    }

    private void RequestSetItemLevel(ItemId itemId, int level)
    {
        if (IsServer)
        {
            SetItemLevelFromServer(itemId, level);
        }
        else
        {
            RequestSetItemLevelServerRpc(itemId, level);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestSetItemLevelServerRpc(ItemId itemId, int level)
    {
        SetItemLevelFromServer(itemId, level);
    }

    [ContextMenu("Find Mouse Inventory")]
    private void FindMouseInventory()
    {
        PlayerItemInventory[] inventories =
            FindObjectsByType<PlayerItemInventory>(FindObjectsSortMode.None);

        foreach (PlayerItemInventory inventory in inventories)
        {
            PlayerRoleState roleState = inventory.GetComponentInParent<PlayerRoleState>();

            if (roleState != null && roleState.GetRole() == PlayerRole.Mouse)
            {
                targetInventory = inventory;
                Debug.Log("[SandboxItemInventoryTester] Found mouse inventory.");
                return;
            }
        }

        Debug.LogWarning("[SandboxItemInventoryTester] Could not find mouse inventory.");
    }

    [ContextMenu("Add Debug Item")]
    private void AddDebugItem()
    {
        RequestAddItem(itemToAdd, amountToAdd);
    }

    [ContextMenu("Set Debug Item Level")]
    private void SetSelectedDebugItemLevel()
    {
        RequestSetItemLevel(itemToAdd, levelToSet);
    }

    private void AddItemFromServer(ItemId itemId, int amount)
    {
        if (!CanModifyInventory())
            return;

        targetInventory.AddItemFromServer(itemId, amount);

        Debug.Log($"[SandboxItemInventoryTester] Added {amount}x {itemId}.");
    }

    private void SetItemLevelFromServer(ItemId itemId, int level)
    {
        if (!CanModifyInventory())
            return;

        targetInventory.SetItemLevelFromServer(itemId, level);

        Debug.Log($"[SandboxItemInventoryTester] Set {itemId} level to {level}.");
    }

    private bool CanModifyInventory()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[SandboxItemInventoryTester] Only the server can modify inventory.");
            return false;
        }

        if (targetInventory == null)
        {
            FindMouseInventory();
        }

        if (targetInventory == null)
        {
            Debug.LogWarning("[SandboxItemInventoryTester] No target inventory assigned.");
            return false;
        }

        return true;
    }
}