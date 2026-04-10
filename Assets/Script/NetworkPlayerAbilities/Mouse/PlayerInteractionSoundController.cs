using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(MouseAbilityController))]
[RequireComponent(typeof(PlayerRoleState))]
[RequireComponent(typeof(PlayerRoleState))]
public class PlayerInteractionSoundController : NetworkBehaviour
{
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactSoundClip;

    private void Awake()
    {
        if (roleState == null)
            roleState = GetComponent<PlayerRoleState>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    [ClientRpc]
    public void PlayInteractionSoundClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;
        if (roleState == null || roleState.GetRole() != PlayerRole.Cat) return;
        if (audioSource == null || interactSoundClip == null) return;

        audioSource.PlayOneShot(interactSoundClip);
    }
}