using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkClockController : NetworkBehaviour
{
    [Header("Clock State Reference")]
    [SerializeField] private NetworkClockState clockState;
    
    [Header("Interaction Rules")]
    [SerializeField] private bool requirePlayerInTrigger = true;

    [SerializeField] private List<ulong> playersNOIdInRange;
    
    public bool TryInteract(NetworkObject interactor)
    {
        if (!IsServer) return false;
        if (interactor == null) return false;
        if (clockState == null) return false;

        if (requirePlayerInTrigger && !playersNOIdInRange.Contains(interactor.NetworkObjectId))
        {
            return false;
        }

        PlayerRoleState roleState = interactor.GetComponent<PlayerRoleState>();
        if (roleState == null) return false;

        switch (roleState.GetRole())
        {
            case PlayerRole.Mouse:
                return TryActivateFromMouse();

            case PlayerRole.Cat:
                return TryResetFromCat();

            default:
                return false;
        }
    }

    private bool TryActivateFromMouse()
    {
        if (!clockState.GetIsClockActive()) return false;
        if (clockState.GetIsBoostActive()) return false;

        clockState.SetBoostActive(true);
        return true;
    }

    private bool TryResetFromCat()
    {
        if (!clockState.GetIsBoostActive() || !clockState.GetIsClockActive()) return false;

        clockState.SetBoostActive(false);
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        if (!other.CompareTag("Player")) return;

        NetworkObject playerNO = other.GetComponent<NetworkObject>();
        if (playerNO == null) return;

        playersNOIdInRange.Add(playerNO.NetworkObjectId);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsServer) return;
        if (!other.CompareTag("Player")) return;

        NetworkObject playerNO = other.GetComponent<NetworkObject>();
        if (playerNO == null) return;

        playersNOIdInRange.Remove(playerNO.NetworkObjectId);
    }

}