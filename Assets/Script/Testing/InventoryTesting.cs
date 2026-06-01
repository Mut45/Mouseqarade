using Unity.Netcode;
using UnityEditor.SearchService;
using UnityEngine;

public class SandboxItemInventoryTester : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerItemInventory targetInventory;

    [Header("Debug Add Settings")]
    [SerializeField] private ItemId itemToAdd = ItemId.SmokeBomb;
    [SerializeField] private int amountToAdd = 3;
    [SerializeField] private int levelToSet = 1;

    private void Update()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddItem(ItemId.SmokeBomb, amountToAdd);
        }

    }

    [ContextMenu("Find Mouse Inventory")]
    private void FindMouseInventory()
    {
        PlayerItemInventory[] inventories = FindObjectsByType<PlayerItemInventory>(FindObjectsSortMode.None);

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
        AddItem(itemToAdd, amountToAdd);
    }

    [ContextMenu("Set Debug Item Level")]
    private void SetSelectedDebugItemLevel()
    {
        if (!CanModifyInventory())
            return;

        targetInventory.SetItemLevelFromServer(itemToAdd, levelToSet);

        Debug.Log($"[SandboxItemInventoryTester] Set {itemToAdd} level to {levelToSet}.");
    }

    private void AddItem(ItemId itemId, int amount)
    {
        if (!CanModifyInventory())
            return;

        targetInventory.AddItemFromServer(itemId, amount);

        Debug.Log($"[SandboxItemInventoryTester] Added {amount}x {itemId}.");
    }

    private bool CanModifyInventory()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("[SandboxItemInventoryTester] Only the server/host can modify inventory.");
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