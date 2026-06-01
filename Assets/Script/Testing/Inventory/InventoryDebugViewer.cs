using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class InventoryDebugViewer : NetworkBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerItemInventory targetInventory;

    [Header("Realtime Debug View")]
    [SerializeField] private List<DebugInventoryEntry> debugEntries = new();

    [Header("Settings")]
    [SerializeField] private bool autoFindMouseInventory = true;
    [SerializeField] private bool refreshEveryFrame = true;


    private void Update()
    {
        if (targetInventory == null && autoFindMouseInventory)
        {
            FindMouseInventory();
        }

        if (refreshEveryFrame)
        {
            RefreshDebugView();
        }
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
                Debug.Log("[InventoryDebugViewer] Found mouse inventory.");
                return;
            }
        }

        //Debug.LogWarning("[InventoryDebugViewer] Could not find mouse inventory.");
    }

    [ContextMenu("Refresh Debug View")]
    private void RefreshDebugView()
    {
        debugEntries.Clear();

        if (targetInventory == null)
            return;

        List<ItemInventoryEntry> entries = targetInventory.GetOwnedItemEntries();

        foreach (ItemInventoryEntry entry in entries)
        {
            debugEntries.Add(new DebugInventoryEntry
            {
                itemId = entry.ItemId,
                count = entry.Count,
                level = entry.Level
            });
        }
    }
}

[System.Serializable]
public class DebugInventoryEntry
{
    public ItemId itemId;
    public int count;
    public int level;
}