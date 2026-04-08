using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CatPrimaryActionController))]
[RequireComponent(typeof(PlayerRoleState))]
public class CatAbilityController : NetworkBehaviour
{
    [SerializeField] private CatPrimaryActionController primaryActionController;
    [SerializeField] private CatSkillController catSkillController;
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private PlayerInteractionController interactionController;
    void Awake()
    {
        if (roleState == null) roleState = GetComponent<PlayerRoleState>();
        if (primaryActionController == null) primaryActionController = GetComponent<CatPrimaryActionController>();
        if (catSkillController == null) catSkillController = GetComponent<CatSkillController>();
    }

    public void HandleInput(PlayerInputNetworkData prevInput, PlayerInputNetworkData currInput)
    {
        if (!IsOwner) return;

        if (roleState == null || roleState.GetRole() != PlayerRole.Cat) return;

        bool primaryJustPressed = currInput.PrimaryPressed && !prevInput.PrimaryPressed;
        if (primaryJustPressed)
        {
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

        bool secondaryJustPressed = currInput.SecondaryPressed && !prevInput.SecondaryPressed;
        if (secondaryJustPressed)
        {
            Debug.Log("[CatAbilityController] Secondary pressed");
            if (catSkillController == null)
            {
                Debug.LogError("[CatAbilityController] catSkillController is NULL");
                return;
            }

            catSkillController.HandleUseSkillInput();
        }

        bool cycleJustPressed = currInput.CyclePressed && !prevInput.CyclePressed;
        if (cycleJustPressed)
        {
            catSkillController.HandleCycleSkillInput();
        }
    }
}