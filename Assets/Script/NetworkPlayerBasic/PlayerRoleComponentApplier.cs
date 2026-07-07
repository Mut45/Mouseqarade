using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Role effect applier to enable/disable componenets depending on the player's current role
/// </summary>
public class PlayerRoleComponentApplier : NetworkBehaviour
{
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private Behaviour[] mouseComponents;
    [SerializeField] private Behaviour[] catComponents;
    [SerializeField] private GameObject[] mouseObjects;
    [SerializeField] private GameObject[] catObjects;

    public override void OnNetworkSpawn()
    {
        if (roleState == null)
        {
            roleState = GetComponent<PlayerRoleState>();
        }

        roleState.OnRoleChanged += ApplyPlayerRole;
        ApplyPlayerRole(roleState.GetRole());
    }

    public override void OnNetworkDespawn()
    {
        if (roleState != null)
        {
            roleState.OnRoleChanged -= ApplyPlayerRole;
        }
    }
        

    public void ApplyPlayerRole(PlayerRole role)
    {
        SetEnabled(mouseComponents, role == PlayerRole.Mouse && IsOwner);
        SetEnabled(catComponents, role == PlayerRole.Cat && IsOwner);

        SetActive(mouseObjects, role == PlayerRole.Mouse && IsOwner);
        SetActive(catObjects, role == PlayerRole.Cat && IsOwner);
    }

    private void SetEnabled(Behaviour[] components, bool enabled)
    {
        foreach (Behaviour component in components)
        {
            if (component != null)
            {
                component.enabled = enabled;
            }
        }
    }

    private void SetActive(GameObject[] objects, bool active)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }
}