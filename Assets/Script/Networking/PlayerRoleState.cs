using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerRoleState : NetworkBehaviour
{
    public NetworkVariable<PlayerRole> Role = new (
        PlayerRole.Mouse,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Ability Controller References")]
    [SerializeField] private CatAbilityController catAbility;
    [SerializeField] private MouseAbilityController mouseAbility;
    private void Awake()
    {
        if (catAbility == null) catAbility = GetComponent<CatAbilityController>();
        if (mouseAbility == null) mouseAbility = GetComponent<MouseAbilityController>();
    }

    public override void OnNetworkSpawn()
    {
        Role.OnValueChanged += OnRoleChanged;
        ApplyGamplayRole(Role.Value);
    }

    public override void OnNetworkDespawn()
    {
        Role.OnValueChanged -= OnRoleChanged;
    }

    /// <summary>
    /// Server only method for setting the role of a player
    /// </summary>
    /// <param name="role"></param>
    public void SetRole(PlayerRole role)
    {
        if (!IsServer) return;
        Role.Value = role;
    }

    private void OnRoleChanged(PlayerRole prevRole, PlayerRole newRole)
    {
        ApplyGamplayRole(newRole);
    }

    private void ApplyGamplayRole(PlayerRole role)
    {
        bool isMouse = role == PlayerRole.Mouse;
        if (catAbility != null)
        {
            catAbility.enabled = !isMouse;
        }
        if (mouseAbility != null)
        {
            mouseAbility.enabled = isMouse;            
        }
    }
}