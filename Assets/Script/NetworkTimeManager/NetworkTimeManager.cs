using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkTimeManager : NetworkBehaviour
{
    [Header("Core Timer Settings")]
    [SerializeField] private float totalTime = 300f;
    [SerializeField] private bool autoStartCountdown = true;


    [Header("UI (Optional Scene Reference)")]
    [SerializeField] private Text remainingTimeText;
    [SerializeField] private bool showMinuteSecondFormat = true;

    [Header("Cycle Settings")]
    [SerializeField] private float ringTotalCycleTime = 30f;

    public event Action OnEveryCycleElapsed;
    [SerializeField] private NetworkVariable<float> syncedRemainingTime = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<bool> syncedIsCountingDown = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<int> syncedCycleCount = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private readonly NetworkVariable<bool> syncedStage = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private float lastCycleTriggerElapsedTime;

    public override void OnNetworkSpawn()
    {
        syncedRemainingTime.OnValueChanged += HandleTimerValueChanged;
        syncedIsCountingDown.OnValueChanged += HandleCountingStateChanged;

        if (IsServer)
        {
            syncedRemainingTime.Value = totalTime;
            syncedIsCountingDown.Value = false;
            syncedCycleCount.Value = 0;
            syncedStage.Value = false;
            lastCycleTriggerElapsedTime = 0f;

            if (autoStartCountdown)
            {
                StartCountdown();
            }
        }

        UpdateRemainingTimeText();
    }

    public override void OnNetworkDespawn()
    {
        syncedRemainingTime.OnValueChanged -= HandleTimerValueChanged;
        syncedIsCountingDown.OnValueChanged -= HandleCountingStateChanged;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (!syncedIsCountingDown.Value) return;

        float timeDrainRate = InGameManager.Instance.GetClockBoostMultiplier();
        float newRemainingTime = syncedRemainingTime.Value - Time.deltaTime * timeDrainRate;
        syncedRemainingTime.Value = Mathf.Max(newRemainingTime, 0f);

        if (syncedRemainingTime.Value <= 0f)
        {
            syncedRemainingTime.Value = 0f;
            StopCountdown();
            Debug.Log("[TimeManager] Countdown finished.");
            return;
        }

        float totalElapsed = totalTime - syncedRemainingTime.Value;
        while (totalElapsed - lastCycleTriggerElapsedTime >= ringTotalCycleTime)
        {
            TriggerCycleElapsed();
            lastCycleTriggerElapsedTime += ringTotalCycleTime;
        }
    }

    private void HandleTimerValueChanged(float previousValue, float currentValue)
    {
        UpdateRemainingTimeText();
    }

    private void HandleCountingStateChanged(bool previousValue, bool currentValue)
    {
        UpdateRemainingTimeText();
    }

    private void UpdateRemainingTimeText()
    {
        if (remainingTimeText == null) return;

        float remaining = syncedRemainingTime.Value;

        string timeText;
        if (remaining <= 0f)
        {
            timeText = "Time Up!";
        }
        else if (showMinuteSecondFormat)
        {
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            timeText = $"{minutes}:{seconds:D2}";
        }
        else
        {
            timeText = $"{remaining:F1}s";
        }

        remainingTimeText.text = $"Remaining Time: {timeText}";
    }

    private void TriggerCycleElapsed()
    {
        syncedCycleCount.Value++;
        syncedStage.Value = syncedCycleCount.Value > 1;

        Debug.Log($"[TimeManager] Cycle triggered. Count={syncedCycleCount.Value}, Stage={syncedStage.Value}");

        OnEveryCycleElapsed?.Invoke();
    }

    #region Exposed public API

    public void StartCountdown()
    {
        if (!IsServer) return;

        syncedRemainingTime.Value = Mathf.Clamp(syncedRemainingTime.Value, 0f, totalTime);
        syncedIsCountingDown.Value = true;
        lastCycleTriggerElapsedTime = totalTime - syncedRemainingTime.Value;

        Debug.Log($"[TimeManager] Countdown started. Total={totalTime}");
    }

    public void StopCountdown()
    {
        if (!IsServer) return;

        syncedIsCountingDown.Value = false;
        Debug.Log("[TimeManager] Countdown stopped.");
    }

    public void ResetCountdown()
    {
        if (!IsServer) return;

        syncedRemainingTime.Value = totalTime;
        syncedIsCountingDown.Value = true;
        syncedCycleCount.Value = 0;
        syncedStage.Value = false;
        lastCycleTriggerElapsedTime = 0f;

        Debug.Log($"[TimeManager] Countdown reset.");
    }

    public void ResetCycleCount()
    {
        if (!IsServer) return;

        syncedCycleCount.Value = 0;
        syncedStage.Value = false;
        lastCycleTriggerElapsedTime = totalTime - syncedRemainingTime.Value;

        Debug.Log("[TimeManager] Cycle count reset.");
    }

    public bool IsCountingDown()
    {
        return syncedIsCountingDown.Value;
    }

    public float GetRemainingTime()
    {
        return syncedRemainingTime.Value;
    }

    public float GetTotalTime()
    {
        return totalTime;
    }

    public float GetRingTotalCycleTime()
    {
        return ringTotalCycleTime;
    }

    public float GetElapsedTimeInCycle()
    {
        float totalElapsed = totalTime - syncedRemainingTime.Value;
        return totalElapsed % ringTotalCycleTime;
    }

    public int GetCycleCount()
    {
        return syncedCycleCount.Value;
    }

    public bool GetStageState()
    {
        return syncedStage.Value;
    }

    #endregion
}