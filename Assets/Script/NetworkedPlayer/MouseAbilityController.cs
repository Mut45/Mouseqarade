using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerAppearanceState))]
public class MouseAbilityController : NetworkBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAppearanceState appearState;

}