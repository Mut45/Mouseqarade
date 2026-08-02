using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerRoleState))]
public class CatProgressionState : NetworkBehaviour
{
    public event Action<int, int> OnLevelChanged;
    public event Action<int, int> OnExpChanged;
    public event Action<int, int> OnPendingUpgradeCountChanged;

    [SerializeField]
    private CatProgressionDefinition progressionDefinition;

    private readonly NetworkVariable<int> currentLevel = new(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> currentExp = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> expToNextLevel = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> pendingUpgradeCount = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private PlayerRoleState roleState;

    public int CurrentLevel => currentLevel.Value;
    public int CurrentExp => currentExp.Value;
    public int ExpToNextLevel => expToNextLevel.Value;
    public int PendingUpgradeCount => pendingUpgradeCount.Value;
    public bool IsMaxLevel =>
        progressionDefinition != null &&
        currentLevel.Value >= progressionDefinition.MaxLevel;

    private void Awake()
    {
        roleState = GetComponent<PlayerRoleState>();
    }

    public override void OnNetworkSpawn()
    {
        currentLevel.OnValueChanged += HandleLevelChanged;
        currentExp.OnValueChanged += HandleExpChanged;
        pendingUpgradeCount.OnValueChanged += HandleUpgradeCountChanged;

        if (IsServer)
        {
            RefreshExpToNextLevel();
        }
    }

    public override void OnNetworkDespawn()
    {
        currentLevel.OnValueChanged -= HandleLevelChanged;
        currentExp.OnValueChanged -= HandleExpChanged;
        pendingUpgradeCount.OnValueChanged -= HandleUpgradeCountChanged;
    }

    public bool GrantExpFromServer(int amount)
    {
        if (!IsServer)
        {
            Debug.LogWarning(
                "[CatProgressionState] Rejected EXP grant outside server.");
            return false;
        }

        if (!IsSpawned || progressionDefinition == null)
            return false;

        if (roleState == null || roleState.GetRole() != PlayerRole.Cat)
        {
            Debug.LogWarning(
                "[CatProgressionState] EXP rejected because player is not Cat.");
            return false;
        }

        if (amount <= 0 || IsMaxLevel)
            return false;

        Debug.Log($"[CatProgressionState] Exp granted: {amount}");
        currentExp.Value += amount;
        ProcessLevelUps();
        return true;
    }

    private void ProcessLevelUps()
    {
        while (!IsMaxLevel)
        {
            int required =
                progressionDefinition.GetExpRequiredForLevel(currentLevel.Value);

            if (required <= 0 || currentExp.Value < required)
                break;

            currentExp.Value -= required;
            currentLevel.Value++;
            pendingUpgradeCount.Value++;
        }

        if (IsMaxLevel)
        {
            currentExp.Value = 0;
        }

        RefreshExpToNextLevel();
    }

    private void RefreshExpToNextLevel()
    {
        expToNextLevel.Value = IsMaxLevel
            ? 0
            : progressionDefinition.GetExpRequiredForLevel(currentLevel.Value);
    }

    private void HandleLevelChanged(int previous, int current)
    {
        OnLevelChanged?.Invoke(previous, current);
    }

    private void HandleExpChanged(int previous, int current)
    {
        OnExpChanged?.Invoke(previous, current);
    }

    private void HandleUpgradeCountChanged(int previous, int current)
    {
        OnPendingUpgradeCountChanged?.Invoke(previous, current);
    }
}