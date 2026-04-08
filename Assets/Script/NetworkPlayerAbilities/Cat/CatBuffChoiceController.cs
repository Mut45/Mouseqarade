using Unity.Netcode;
using UnityEngine;

public enum CatSkillType
{
    RedLightGreenLight = 0,
    // Future:
    // Dash = 1,
    // RevealMouse = 2,
}

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(PlayerRoleState))]
[RequireComponent(typeof(NetworkedPlayerController))]
public class CatBuffChoiceController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private NetworkedPlayerController playerController;

    private bool isBuffPanelOpen;

    private void Awake()
    {
        if (roleState == null)
        {
            roleState = GetComponent<PlayerRoleState>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<NetworkedPlayerController>();
        }
    }

    public override void OnNetworkSpawn()
    {

    }

    public void BeginBuffChoiceFromServer()
    {
        if (!IsServer) return;
        if (roleState == null || roleState.GetRole() != PlayerRole.Cat) return;
        if (playerController == null) return;
        if (isBuffPanelOpen) return;

        playerController.SetMovementLock(true);

        ShowBuffChoiceClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { OwnerClientId }
            }
        });
    }

    [ClientRpc]
    private void ShowBuffChoiceClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        if (roleState == null || roleState.GetRole() != PlayerRole.Cat) return;

        if (BuffCardSpawnManager.Instance== null)
        {
            Debug.LogWarning("[CatBuffChoiceController] No BuffCardSpawnManager.Instance found on local client.");
            return;
        }

        isBuffPanelOpen = true;
        BuffCardSpawnManager.Instance.SetActiveChoiceController(this);
        BuffCardSpawnManager.Instance.SpawnBuffForCurrentRound();
    }

    public void NotifyBuffSelectionFinished()
    {
        if (!IsOwner) return;

        if (BuffCardSpawnManager.Instance != null)
        {
            BuffCardSpawnManager.Instance.CloseCurrentPanel();
            BuffCardSpawnManager.Instance.ClearActiveChoiceController(this);
        }

        NotifyBuffSelectionFinishedServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void NotifyBuffSelectionFinishedServerRpc()
    {
        if (playerController != null)
        {
            playerController.SetMovementLock(false);
        }

        isBuffPanelOpen = false;
    }
}