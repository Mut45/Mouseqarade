using UnityEngine;
using System;
using Unity.Netcode;
/// <summary>
/// Static class for invoking different exp reward events
/// </summary>

public enum CatExpRewardEventType
{
    PassiveTime = 0,
    FailedCatch = 1,
    MouseCaught = 2,
    TrapDisabled = 3
}
public readonly struct CatExpRewardEvent
{
    public CatExpRewardEventType Type { get; }
    public ulong SourceNetworkObjectId { get; }
    public ulong InstigatorClientId { get; }

    public CatExpRewardEvent(
        CatExpRewardEventType type,
        ulong sourceNetworkObjectId,
        ulong instigatorClientId)
    {
        Type = type;
        SourceNetworkObjectId = sourceNetworkObjectId;
        InstigatorClientId = instigatorClientId;
    }
}
public static class CatExpRewardEvents
{
    public static event Action<CatExpRewardEvent> RewardValidated;

    public static void PublishFromServer(CatExpRewardEvent rewardEvent)
    {   
        RewardValidated?.Invoke(rewardEvent);
    }
}