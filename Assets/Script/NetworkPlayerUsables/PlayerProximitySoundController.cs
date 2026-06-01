using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerRoleState))]
public class PlayerProximitySoundController : NetworkBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip mouseProximityClip;

    [Header("Detection")]
    [SerializeField] private float proximityDistance = 3.0f;
    [SerializeField] private bool triggerOnEnterOnly = true;
    [SerializeField] private float proximityCooldown = 2.0f;

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = false;

    private PlayerRoleState localRoleState;
    private Transform cachedCatTransform;
    private Transform cachedMouseTransform;

    private bool wasInProximity;
    private float nextProximityTime;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        localRoleState = GetComponent<PlayerRoleState>();
    }

    public override void OnNetworkSpawn()
    {
        RefreshTrackedPlayers();
        wasInProximity = false;
        nextProximityTime = 0f;
    }

    private void Update()
    {
        if (!IsSpawned) return;
        if (!IsOwner) return;
        if (localRoleState == null) return;
        if (localRoleState.GetRole() != PlayerRole.Cat) return;

        if (NetworkRoleBuffState.Instance == null) return;
        if (!NetworkRoleBuffState.Instance.HasBuffForRole(PlayerRole.Cat, BuffCardEffectId.MouseProximitySound))
            return;

        if (cachedCatTransform == null || cachedMouseTransform == null)
        {
            RefreshTrackedPlayers();
            if (cachedCatTransform == null || cachedMouseTransform == null) return;
        }

        TickmouseProximitySound();
    }

    private void TickmouseProximitySound()
    {
        if (audioSource == null || mouseProximityClip == null) return;

        float dist = Vector2.Distance(
            cachedMouseTransform.position,
            cachedCatTransform.position
        );

        bool inRange = dist <= proximityDistance;

        if (triggerOnEnterOnly)
        {
            if (inRange && !wasInProximity)
            {
                TryPlayProximitySound();
            }
        }
        else
        {
            if (inRange)
            {
                TryPlayProximitySound();
            }
        }

        wasInProximity = inRange;
    }

    private void TryPlayProximitySound()
    {
        if (Time.time < nextProximityTime) return;

        audioSource.PlayOneShot(mouseProximityClip);
        nextProximityTime = Time.time + Mathf.Max(0.05f, proximityCooldown);

        if (logDebugMessages)
        {
            Debug.Log("[PlayerProximitySoundController] Played mouse proximity sound for local cat player.");
        }
    }

    private void RefreshTrackedPlayers()
    {
        cachedCatTransform = null;
        cachedMouseTransform = null;

        PlayerRoleState[] players = FindObjectsByType<PlayerRoleState>(FindObjectsSortMode.None);

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null || !players[i].IsSpawned)
                continue;

            PlayerRole role = players[i].GetRole();

            if (role == PlayerRole.Cat && cachedCatTransform == null)
            {
                cachedCatTransform = players[i].transform;
            }
            else if (role == PlayerRole.Mouse && cachedMouseTransform == null)
            {
                cachedMouseTransform = players[i].transform;
            }
        }

    }
}