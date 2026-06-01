using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Item Runtime Effects Applicator - Server Authoritative
/// </summary>
[RequireComponent(typeof(PlayerItemInventory))]
public class ItemUseController : NetworkBehaviour
{
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private PlayerItemInventory inventory;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<PlayerItemInventory>();
        }

    }

    #region Item Use Flow
    public void RequestUseItem(ItemUseNetworkRequestData request)
    {
        if (!IsOwner) return;

        if (IsServer)
        {
            UseItemFromServer(request);
        }
        else
        {
            UseItemViaServerRPC(request);
        }
    }

    private void UseItemFromServer(ItemUseNetworkRequestData request)
    {
        if (!IsServer) return;

        if (itemDatabase == null) return;

        if (inventory == null) return;

        if (request.ItemId == ItemId.None) return;

        if (!itemDatabase.TryGetDefinitionAndEffectById(request.ItemId, out ItemDefinition definition, out IUsableItem itemEffect))
        {
            Debug.LogWarning($"[ItemUseController] No item pair found for {request.ItemId}.");
            return;
        }

        if (!inventory.HasItem(request.ItemId))
        {
            Debug.Log($"[ItemUseController] Player does not have item: {request.ItemId}");
            return;
        }

        // Consume item from inventory
        inventory.ConsumeItemFromServer(request.ItemId, 1);

        // Prepare item context for usage
        int itemLevel = inventory.GetItemLevel(request.ItemId);

        // Call use server form the effect to apply the server-wide effect
        ItemUseContext context = new ItemUseContext
        {
            UserTransform = transform,
            UserClientId = OwnerClientId,
            UserPosition = transform.position,
            Request = request,
            Definition = definition,
            ItemLevel = itemLevel,

        };

        itemEffect.UseServer(context);
        
        // TODO: Apply VFX 
    }

    [ServerRpc]
    private void UseItemViaServerRPC(ItemUseNetworkRequestData request)
    {
        UseItemFromServer(request);
    }

    #endregion
}