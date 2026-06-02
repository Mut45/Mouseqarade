using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerDisguiseState))]
[RequireComponent(typeof(PlayerRoleState))]
[RequireComponent(typeof(PlayerTauntState))]
public class MouseAbilityController : NetworkBehaviour
{
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private PlayerDisguiseState disguiseState;
    [SerializeField] private PlayerTauntState tauntState;
    [SerializeField] private PlayerInteractionController interactionController;

    void Awake()
    {
        if (roleState == null) roleState = GetComponent<PlayerRoleState>();
        if (disguiseState == null) disguiseState = GetComponent<PlayerDisguiseState>();
        if (tauntState == null) tauntState = GetComponent<PlayerTauntState>();
    }
    public void HandleInput(PlayerInputNetworkData prevInput, PlayerInputNetworkData currInput)
    {
        if (!IsServer) return;

        if (roleState.GetRole() != PlayerRole.Mouse) return;

        bool primaryJustPressed = currInput.PrimaryPressed && !prevInput.PrimaryPressed;
        if (primaryJustPressed)
        {
            TryUseDisguise();
        }

        bool interactJustPressed = currInput.InteractPressed && !prevInput.InteractPressed;
        if (interactJustPressed)
        {
            if (interactionController.TryGetCurrentClockTarget(out NetworkClockController clock))
            {
                clock.TryInteract(NetworkObject);
            }
        }

        bool extraJustPressed = currInput.ExtraPressed && !prevInput.ExtraPressed;
        if (extraJustPressed)
        {
            tauntState.TryStartTaunt();
        }

    }

    private void TryUseDisguise()
    {
        if (!interactionController.TryGetCurrentNpcTarget(out NetworkNPCController npc))
        {
            Debug.Log("[Player Disguise] Try player disguise failed");
            return;
        }

        disguiseState.SetDisguise(true, npc.NetworkObjectId);
        NetworkRoleBuffSystem.Instance?.NotifyMouseInteraction();
    }

}