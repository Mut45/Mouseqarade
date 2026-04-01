using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerDisguiseState))]
[RequireComponent(typeof(PlayerRoleState))]
public class MouseAbilityController : NetworkBehaviour
{
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private PlayerDisguiseState disguiseState;
    [SerializeField] private PlayerInteractionController interactionController;


    public void HandleInput(PlayerInputNetworkData prevInput, PlayerInputNetworkData currInput)
    {
        if (!IsServer) return;
        if (roleState.GetRole() != PlayerRole.Mouse) return;
        bool justPressed = currInput.PrimaryPressed && !prevInput.PrimaryPressed;
        if (!justPressed) return;
        TryUseDisguise();

    }

    private void TryUseDisguise()
    {
        if (!interactionController.TryGetCurrentNpcTarget(out NetworkNPCController npc))
        {
            Debug.Log("[Player Disguise] Try player disguise failed");
            return;
        }
        disguiseState.SetDisguise(true, npc.NetworkObjectId);
    }
}