using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkClockState : NetworkBehaviour
{
    private NetworkVariable<bool> isClockActive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<bool> isBoostActive = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public event Action<bool> OnClockActiveChanged;
    public event Action<bool> OnBoostActiveChanged;
    public event Action<bool, bool> OnClockStateChanged;

    public override void OnNetworkSpawn()
    {
        isClockActive.OnValueChanged += HandleClockActiveChanged;
        isBoostActive.OnValueChanged += HandleBoostActiveChanged;

        OnClockActiveChanged?.Invoke(isClockActive.Value);
        OnBoostActiveChanged?.Invoke(isBoostActive.Value);
        OnClockStateChanged?.Invoke(isClockActive.Value, isBoostActive.Value);
    }

    public override void OnNetworkDespawn()
    {
        isClockActive.OnValueChanged -= HandleClockActiveChanged;
        isBoostActive.OnValueChanged -= HandleBoostActiveChanged;
    }

    #region Clock active and Boost active handlers
    private void HandleClockActiveChanged(bool prevValue, bool currValue)
    {
        OnClockActiveChanged?.Invoke(currValue);
        OnClockStateChanged?.Invoke(isClockActive.Value, isBoostActive.Value);
    }

    private void HandleBoostActiveChanged(bool prevValue, bool currValue)
    {
        OnBoostActiveChanged?.Invoke(currValue);
        OnClockStateChanged?.Invoke(isClockActive.Value, isBoostActive.Value);
    }
    #endregion

    #region Exposed getters/setters for the network variables
    public bool GetIsClockActive()
    {
        return isClockActive.Value;
    }

    public bool GetIsBoostActive()
    {
        return isBoostActive.Value;
    }

    public void SetIsClockActive(bool active)
    {
        if (IsServer)
        {
            SetClockActiveFromServer(active);
            return;
        }

        SetClockActiveViaServerRpc(active);
    }

    public void SetBoostActive(bool active)
    {
        if (IsServer)
        {
            SetBoostActiveFromServer(active);
            return;
        }

        SetBoostActiveViaServerRpc(active);
    }
    #endregion

    #region Network variables setters from servers & Server rpcs
    private void SetClockActiveFromServer(bool active)
    {
        if (isClockActive.Value == active) return;

        isClockActive.Value = active;

        if (!active && isBoostActive.Value)
        {
            isBoostActive.Value = false;
        }
    }
    [ServerRpc]
    private void SetClockActiveViaServerRpc(bool active)
    {
        SetClockActiveFromServer(active);
    }

    private void SetBoostActiveFromServer(bool active)
    {
        if (!isClockActive.Value && active) return;
        if (isBoostActive.Value == active) return;

        isBoostActive.Value = active;
    }
    [ServerRpc]
    private void SetBoostActiveViaServerRpc(bool active)
    {
        SetBoostActiveFromServer(active);
    }
    #endregion
}