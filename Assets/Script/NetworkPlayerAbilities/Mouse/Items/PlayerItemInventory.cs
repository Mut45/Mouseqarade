using UnityEngine;
using System;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using System.Collections.Generic;

/// <summary>
/// Inventory management - Server-authoritative
/// Contains functions only executable from server
/// Rpc calls dispatching needed before reaching here.
/// </summary>
public class PlayerItemInventory : NetworkBehaviour
{
    [SerializeField] private ItemDatabase itemDB;
    private NetworkList<ItemInventoryEntry> items;
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (itemDB == null) itemDB = GetComponent<ItemDatabase>();

        items = new NetworkList<ItemInventoryEntry>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    }

    public override void OnNetworkSpawn()
    {
        items.OnListChanged += HandleInventoryListChanged;
    }

    public override void OnNetworkDespawn()
    {
        items.OnListChanged -= HandleInventoryListChanged;
    }

    private void HandleInventoryListChanged(NetworkListEvent<ItemInventoryEntry> changeEvent)
    {
        OnInventoryChanged?.Invoke();
    }

    #region Exposed public functions for data about items
    public bool HasItem(ItemId itemId)
    {
        return GetItemCount(itemId) > 0;
    }

    public int GetItemCount(ItemId itemId)
    {
        int index = FindItemIndex(itemId);
        if (index < 0)
        {
            return 0;
        }

        return items[index].Count; 
    }

    public int GetItemLevel(ItemId itemId)
    {
        int index = FindItemIndex(itemId);

        if (index < 0)
        {
            return -1;
        }

        return items[index].Level;
    }

    public bool TryGetEntry(ItemId itemId, out ItemInventoryEntry entry)
    {
        int index = FindItemIndex(itemId);

        if (index < 0)
        {
            entry = default;
            return false;
        }

        entry = items[index];
        return true;
    }

    public List<ItemInventoryEntry> GetOwnedItemEntries()
    {
        List<ItemInventoryEntry> ownedItems = new();
        
        for (int i = 0; i < items.Count; i++)
        {
            ItemInventoryEntry entry = items[i];

            if (entry.ItemId == ItemId.None)
                continue;

            if (entry.Count <= 0)
                continue;

            ownedItems.Add(entry);
        }

        return ownedItems;
    }
    #endregion

    #region Exposed public functions(only from server) for server-authoritative item list modifying
    public void AddItemFromServer(ItemId itemId, int amount)
    {
        if (!IsServer) return;
        if (itemId == ItemId.None) return;
        if (amount <= 0) return;

        int index = FindItemIndex(itemId);
        if (index < 0)
        {
            items.Add(new ItemInventoryEntry(itemId, amount, 1));
            return;
        }

        ItemInventoryEntry entry = items[index];
        entry.Count += amount;
        items[index] = entry;
    }

    public bool ConsumeItemFromServer(ItemId itemId, int amount)
    {
        if (!IsServer) return false;
        if (itemId == ItemId.None) return false;
        if (amount <= 0) return false;

        int index = FindItemIndex(itemId);
        if (index < 0)
            return false;

        ItemInventoryEntry entry = items[index];
        if (entry.Count < amount)
            return false;

        entry.Count -= amount;
        items[index] = entry;

        return true;
    }

    public void SetItemLevelFromServer(ItemId itemId, int level)
    {
        if (!IsServer) return;
        if (itemId == ItemId.None) return;

        int possibleLevel = Mathf.Max(1, level);
        int index = FindItemIndex(itemId);

        if (index < 0)
        {
            items.Add(new ItemInventoryEntry(itemId, 0, possibleLevel));
            return;
        }

        ItemInventoryEntry entry = items[index];
        entry.Level = possibleLevel;
        items[index] = entry;
    }
    #endregion

    #region Helper functions
    private int FindItemIndex(ItemId itemId)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].ItemId == itemId)
                return i;
        }

        return -1;
    }
    #endregion

}