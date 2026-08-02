using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CatPrimaryActionController))]
[RequireComponent(typeof(PlayerRoleState))]
public class CatAbilityController : NetworkBehaviour
{
    [SerializeField] private CatPrimaryActionController primaryActionController;
    //[SerializeField] private CatSkillController catSkillController;
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private PlayerInteractionController interactionController;
    void Awake()
    {
        if (roleState == null) roleState = GetComponent<PlayerRoleState>();
        if (primaryActionController == null) primaryActionController = GetComponent<CatPrimaryActionController>();
    }

    public void HandleInput(PlayerInputNetworkData prevInput, PlayerInputNetworkData currInput)
    {
        if (!IsServer) return;

        if (roleState == null || roleState.GetRole() != PlayerRole.Cat) return;

        bool primaryJustPressed = currInput.PrimaryPressed && !prevInput.PrimaryPressed;
        if (primaryJustPressed)
        {
            // Debug.Log("[CatAbilityController] Primary pressed");
            primaryActionController.TryPrimaryAction();
        }

        bool interactJustPressed = currInput.InteractPressed && !prevInput.InteractPressed;
        if (interactJustPressed)
        {
            if (interactionController.TryGetCurrentClockTarget(out NetworkClockController clock))
            {
                clock.TryInteract(NetworkObject);
            }
        }


    }
}