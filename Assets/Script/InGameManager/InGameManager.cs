using Unity.Netcode;
using UnityEngine;
using System;

[RequireComponent(typeof(NetworkObject))]
public class InGameManager : NetworkBehaviour
{
    public static InGameManager Instance {get; private set;}
    [SerializeField] private NetworkVariable<float> clockBoostMultiplier = new(
        1f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public event Action<float> OnClockBoostMultiplierChanged;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public override void OnNetworkSpawn()
    {
        clockBoostMultiplier.OnValueChanged += HandleClockBoostMultiplierChanged;
        OnClockBoostMultiplierChanged?.Invoke(clockBoostMultiplier.Value);
    }

    public override void OnNetworkDespawn()
    {
        clockBoostMultiplier.OnValueChanged -= HandleClockBoostMultiplierChanged;
    }

    #region Clock boost
    private void HandleClockBoostMultiplierChanged(float oldValue, float newValue)
    {
        OnClockBoostMultiplierChanged?.Invoke(newValue);
    }

    public float GetClockBoostMultiplier()
    {
        return clockBoostMultiplier.Value;
    }

    public bool IsClockBoostActive()
    {
        return clockBoostMultiplier.Value > 1f;
    }

    public void ActivateClockBoost()
    {
        if (!IsServer) return;
        clockBoostMultiplier.Value = 2.0f;
    }

    public void DeactivateClockBoost()
    {
        if (!IsServer) return;
        clockBoostMultiplier.Value = 1.0f;
    }

    #endregion
    
    #region Winning situation handlers
    public void OnMouseBeingCaught()
    {
        if (!IsServer) return;
        Debug.Log("[InGameManager] Cat wins");
    }
    #endregion
}