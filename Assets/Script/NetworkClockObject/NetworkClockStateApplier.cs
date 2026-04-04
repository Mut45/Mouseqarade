
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkClockState))]
public class NetworkClockStateApplier : MonoBehaviour
{
    [SerializeField] private NetworkClockState clockState;

    private void Awake()
    {
        if (clockState == null)
        {
            clockState = GetComponent<NetworkClockState>();
        }
    }

    private void OnEnable()
    {
        if (clockState != null)
        {
            clockState.OnBoostActiveChanged += HandleBoostActiveChanged;
            HandleBoostActiveChanged(clockState.GetIsBoostActive());
        }
    }

    private void OnDisable()
    {
        if (clockState != null)
        {
            clockState.OnBoostActiveChanged -= HandleBoostActiveChanged;
        }
    }
    
    private void HandleBoostActiveChanged(bool isBoostActive)
    {
        if (InGameManager.Instance == null) return;
        if (!InGameManager.Instance.IsServer) return;

        if (isBoostActive)
        {
            InGameManager.Instance.ActivateClockBoost();
        }
        else
        {
            InGameManager.Instance.DeactivateClockBoost();
        }
    }
}