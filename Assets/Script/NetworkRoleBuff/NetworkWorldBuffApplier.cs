using UnityEngine;

public class NetworkWorldBuffApplier : MonoBehaviour
{
    
    [SerializeField] private NetworkRoleBuffState buffState;

    [Header("World References")]
    [SerializeField] private NetworkClockController clockController;
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private CountDownUI countDownUI;

    private bool wasRedLightGreenLightActive;
    private void OnEnable()
    {
        if (buffState != null)
        {
            buffState.OnRuntimeStateChanged += ApplyWorldState;
        }
    }

    private void OnDisable()
    {
        if (buffState != null)
        {
            buffState.OnRuntimeStateChanged -= ApplyWorldState;
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
        if (clockController == null || buffState == null) return;

       // clockController.(!buffState.IsClockDisabled);
    }


    [ContextMenu("Test/Apply red light green light state")]
    private void ApplyRedLightGreenLightState()
    {
        if (buffState == null) return;

        bool isActive = buffState.IsRedLightGreenLightActive;

        if (isActive && !wasRedLightGreenLightActive)
        {
            if (countDownUI != null)
            {
                countDownUI.StartCountdown();
            }

            if (npcManager != null)
            {
                double remainingDuration = buffState.RedLightGreenLightEndTime - Unity.Netcode.NetworkManager.Singleton.ServerTime.Time;
                float duration = Mathf.Max(0f, (float)remainingDuration);
                npcManager.PauseAndThenResume(duration);
            }
        }

        wasRedLightGreenLightActive = isActive;
    }

}