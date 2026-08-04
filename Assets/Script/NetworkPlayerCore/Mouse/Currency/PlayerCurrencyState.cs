using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Methods to modify currency are server-side. RPC dispatching should happen before reaching here.
/// </summary>
public class PlayerCurrencyState : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRoleState roleState;
    public event Action<int> OnCurrencyGained;
    public event Action<int, int> OnCurrencyChanged;
    private NetworkVariable<int> currentCurrency = new(
        0,
        NetworkVariableReadPermission.Owner,
        NetworkVariableWritePermission.Server
    );
    public override void OnNetworkSpawn()
    {
        currentCurrency.OnValueChanged += HandleCurrencyChanged;
        OnCurrencyChanged?.Invoke(currentCurrency.Value, currentCurrency.Value);
    }

    public override void OnNetworkDespawn()
    {
        currentCurrency.OnValueChanged -= HandleCurrencyChanged;
    }

    private void Awake()
    {
        if (roleState == null) roleState = GetComponent<PlayerRoleState>();
    }

    
    #region Exposed Getters & Checkers
    public int GetCurrency()
    {
        return currentCurrency.Value;
    }

    public bool CanAfford(int amount)
    {
        return amount > 0 && currentCurrency.Value >= amount;
    }
    #endregion 

    #region Server-side functions for currency state management
    public void ReduceCurrencyFromServer(int amount)
    {
        if (!IsServer) return;
        if (!IsMouse()) return;
        if (amount <= 0) return;

        currentCurrency.Value = Mathf.Max(0, currentCurrency.Value - amount);
    }

    public void AddCurrencyFromServer(int amount)
    {
        if (!IsServer) return;
        if (!IsMouse()) return;
        if (amount <= 0) return;

        currentCurrency.Value += amount;
    }

    public void AddRandomCurrencyFromServer(int min, int max)
    {
        if (!IsServer) return;
        if (!IsMouse()) return;

        if (max < min)
        {
            int temp = min;
            min = max;
            max = temp;
        }

        int amount = UnityEngine.Random.Range(min, max + 1);
        AddCurrencyFromServer(amount);
    }

    public void ResetCurrencyFromServer()
    {
        if (!IsServer) return;
        if (!IsMouse()) return;

        currentCurrency.Value = 0;
    }

    public bool TrySpendCurrencyFromServer(int amount)
    {
        if (!IsServer) return false;
        if (!IsMouse()) return false;
        if (amount <= 0) return false;
        if (currentCurrency.Value < amount) return false;

        currentCurrency.Value -= amount;
        return true;
    }
    #endregion
    
    private void HandleCurrencyChanged(int previousValue, int currentValue)
    {
        OnCurrencyChanged?.Invoke(previousValue, currentValue);

        int gainedAmount = currentValue - previousValue;
        if (gainedAmount > 0)
        {
            OnCurrencyGained?.Invoke(gainedAmount);
        }
    }
    
    #region Helpers
    private bool IsMouse()
    {
        return roleState != null && roleState.GetRole() == PlayerRole.Mouse;
    }
    #endregion

}