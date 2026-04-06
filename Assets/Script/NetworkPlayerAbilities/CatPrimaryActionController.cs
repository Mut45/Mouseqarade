using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CatPrimaryActionController : NetworkBehaviour
{
    public event Action<CatPrimaryActionController> OnFailedCatchNpc;

    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private NetworkedPlayerController playerController;
    [SerializeField] private PlayerInteractionController interactionController;
    [SerializeField] private float lockDuration = 5.0f;

    [Header("Resolving Target VFX")]
    [SerializeField] private ThumbSpinner thumbSpinnerPrefab;
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 1.2f, 0f);

    public void TryPrimaryAction()
    {
        if (!IsOwner) return;

        if (IsServer)
        {
            ExecuteCatchFromServer();
        }
        else
        {
            ExecuteCatchViaServerRpc();
        }
    }

    [ServerRpc]
    private void ExecuteCatchViaServerRpc()
    {
        ExecuteCatchFromServer();
    }

    private void ExecuteCatchFromServer()
    {
        if (!IsServer) return;
        if (roleState == null || roleState.GetRole() != PlayerRole.Cat) return;

        NetworkedPlayerController currentPlayerTarget = interactionController.GetCurrentPlayerTarget();
        if (currentPlayerTarget != null)
        {
            PlayerRoleState targetPlayerRoleState = currentPlayerTarget.NetworkObject.GetComponent<PlayerRoleState>();
            if (targetPlayerRoleState != null && targetPlayerRoleState.GetRole() == PlayerRole.Mouse)
            {
                ExecuteCatchMouse(currentPlayerTarget);
                return;
            }
        }

        if (interactionController.TryGetCurrentNpcTarget(out NetworkNPCController currentNpcTarget))
        {
            ExecuteCatchNpc(currentNpcTarget);
        }
    }

    private void ExecuteCatchMouse(NetworkedPlayerController mouseTarget)
    {
        if (mouseTarget == null) return;

        playerController.SetMovementLock(true);
        mouseTarget.SetMovementLock(true);
        ShowThumbSpinnerViaClientRpc(mouseTarget.NetworkObjectId, true);
        StartCoroutine(FinishCatchMouseAfterDelay(mouseTarget));
    }

    private void ExecuteCatchNpc(NetworkNPCController npcTarget)
    {
        if (npcTarget == null) return;

        playerController.SetMovementLock(true);
        npcTarget.PauseMovement();
        ShowThumbSpinnerViaClientRpc(npcTarget.NetworkObjectId, false);
        StartCoroutine(FinishCatchNpcAfterDelay(npcTarget));
    }

    private IEnumerator FinishCatchMouseAfterDelay(NetworkedPlayerController mousePlayer)
    {
        yield return new WaitForSeconds(lockDuration);

        playerController.SetMovementLock(false);

        if (mousePlayer != null)
        {
            mousePlayer.SetMovementLock(false);
        }

        if (InGameManager.Instance != null)
        {
            InGameManager.Instance.OnMouseBeingCaught();
        }
    }

    private IEnumerator FinishCatchNpcAfterDelay(NetworkNPCController npcTarget)
    {
        yield return new WaitForSeconds(lockDuration);

        playerController.SetMovementLock(false);

        if (npcTarget != null)
        {
            npcTarget.ResumeMovement();
        }

        if (NetworkRoleBuffSystem.Instance != null)
        {
            NetworkRoleBuffSystem.Instance.NotifyCatFailedCatch();
        }
    }

    [ClientRpc]
    private void ShowThumbSpinnerViaClientRpc(ulong targetNetworkObjectId, bool isTargetMouse)
    {
        if (thumbSpinnerPrefab != null &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetNO))
        {
            Transform target = targetNO.transform;
            var popup = Instantiate(thumbSpinnerPrefab, target);
            popup.transform.localPosition = popupOffset;
            popup.transform.localRotation = Quaternion.identity;
            popup.Init(isTargetMouse);
        }
    }
}