using Unity.Netcode;
using UnityEngine;

public class NetworkWorldBuffApplier : NetworkBehaviour
{
    [Header("World References")]
    [SerializeField] private NetworkClockState clockState;
    //[SerializeField] private NetworkNPCManager npcManager;
    //[SerializeField] private CountDownUI countDownUI;

    private bool wasRedLightGreenLightActive;

    public override void OnNetworkSpawn()
    {
        if (NetworkRoleBuffState.Instance != null)
        {
            NetworkRoleBuffState.Instance.OnRuntimeStateChanged += ApplyWorldState;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (NetworkRoleBuffState.Instance != null)
        {
            NetworkRoleBuffState.Instance.OnRuntimeStateChanged -= ApplyWorldState;
        }
    }

    void Start()
    {
        ApplyWorldState();
    }

    private void ApplyWorldState()
    {
        ApplyClockState();
        ApplyRedLightGreenLightState();
    }

    [ContextMenu("Test/Apply clock state")]
    private void ApplyClockState()
    {
        if (!IsServer) return;
        if (clockState == null || NetworkRoleBuffState.Instance == null) return;
        Debug.Log("[NetworkWorldBuffApplier] the clockstate is applied");
        clockState.SetIsClockActive(!NetworkRoleBuffState.Instance.IsClockDisabled);
    }

    [ContextMenu("Test/Apply red light green light state")]
    private void ApplyRedLightGreenLightState()
    {
        if (NetworkRoleBuffState.Instance == null) return;

        bool isActive = NetworkRoleBuffState.Instance.IsRedLightGreenLightActive;
        CountDownUI countDownUI = FindCatCountdownUI();

        if (isActive && !wasRedLightGreenLightActive)
        {
            if (countDownUI != null)
            {
                countDownUI.StartCountdown();
            }
        }

        wasRedLightGreenLightActive = isActive;
    }

    private CountDownUI FindCatCountdownUI()
    {
        PlayerRoleState[] players = FindObjectsByType<PlayerRoleState>(FindObjectsSortMode.None);

        foreach (PlayerRoleState player in players)
        {
            if (player == null) continue;
            if (!player.IsSpawned) continue;
            if (player.GetRole() != PlayerRole.Cat) continue;

            CountDownUI countdown = player.GetComponentInChildren<CountDownUI>(true);
            if (countdown != null)
            {
                return countdown;
            }
        }

        return null;
    }

}