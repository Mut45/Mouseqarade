using UnityEngine;

public class NetworkNpcMovementBuffApplier : MonoBehaviour
{
    // [SerializeField] private NetworkClockState clockState;
    [SerializeField] private NetworkNPCController npcController;

    private void Awake()
    {
        if (npcController == null) npcController = GetComponent<NetworkNPCController>();
    }
    private void OnEnable()
    {
        if (InGameManager.Instance != null)
        {
            InGameManager.Instance.OnClockBoostMultiplierChanged += HandleClockBoostChanged;            
        }
    }

    private void OnDisable()
    {
        InGameManager.Instance.OnClockBoostMultiplierChanged -= HandleClockBoostChanged;
    }

    private void HandleClockBoostChanged(float clockBoostMultiplier)
    {
        npcController.SetMovementSpeedMultiplier(clockBoostMultiplier);
    }
}