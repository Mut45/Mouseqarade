using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CatPrimaryActionController))]
[RequireComponent(typeof(PlayerRoleState))]
public class CatAbilityController : NetworkBehaviour
{
    [SerializeField] private CatPrimaryActionController primaryActionController;
    [SerializeField] private PlayerRoleState roleState;

    void Awake()
    {
        if (roleState == null) roleState = GetComponent<PlayerRoleState>();
        if (primaryActionController == null) primaryActionController = GetComponent<CatPrimaryActionController>();
    }

    public void HandleInput(PlayerInputNetworkData prevInput, PlayerInputNetworkData currInput)
    {
        if (!IsOwner) return;

        if (roleState == null || roleState.GetRole() != PlayerRole.Cat) return;

        bool justPressed = currInput.PrimaryPressed && !prevInput.PrimaryPressed;
        if (!justPressed) return;

        primaryActionController.TryPrimaryAction();
    }
}