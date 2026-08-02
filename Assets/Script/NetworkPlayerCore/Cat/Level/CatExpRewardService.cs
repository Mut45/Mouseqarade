using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CatExpRewardService : NetworkBehaviour
{
    [SerializeField]
    private List<CatExpRewardDefinition> rewardDefinitions = new();

    private readonly Dictionary<
        CatExpRewardEventType,
        CatExpRewardDefinition> rewardLookup = new();

    private void Awake()
    {
        BuildRewardLookup();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        CatExpRewardEvents.RewardValidated += HandleRewardValidated;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            CatExpRewardEvents.RewardValidated -= HandleRewardValidated;
        }
    }

    private void BuildRewardLookup()
    {
        rewardLookup.Clear();

        foreach (CatExpRewardDefinition definition in rewardDefinitions)
        {
            if (definition == null)
                continue;

            if (!rewardLookup.TryAdd(definition.EventType, definition))
            {
                Debug.LogError(
                    $"[CatExpRewardService] Duplicate reward definition: " +
                    $"{definition.EventType}");
            }
        }
    }

    private void HandleRewardValidated(CatExpRewardEvent rewardEvent)
    {
        if (!IsServer)
            return;

        if (!rewardLookup.TryGetValue(
                rewardEvent.Type,
                out CatExpRewardDefinition definition))
        {
            Debug.LogWarning(
                $"[CatExpRewardService] No definition for " +
                $"{rewardEvent.Type}.");
            return;
        }

        if (!NetworkLookUp.TryGetPlayerObjectByRole(
                NetworkManager,
                PlayerRole.Cat,
                out NetworkObject catPlayer))
        {
            Debug.LogWarning(
                "[CatExpRewardService] Could not find Cat player.");
            return;
        }

        if (catPlayer.OwnerClientId != rewardEvent.InstigatorClientId)
        {
            Debug.LogWarning(
                $"[CatExpRewardService] Reward instigator " +
                $"{rewardEvent.InstigatorClientId} is not current Cat.");
            return;
        }

        if (!catPlayer.TryGetComponent(
                out CatProgressionState progressionState))
        {
            Debug.LogError(
                "[CatExpRewardService] Cat player has no progression state.");
            return;
        }

        bool granted =
            progressionState.GrantExpFromServer(definition.ExpAmount);

        Debug.Log(
            $"[CatExp] Type={rewardEvent.Type}, " +
            $"Amount={definition.ExpAmount}, " +
            $"Instigator={rewardEvent.InstigatorClientId}, " +
            $"Granted={granted}, " +
            $"Level={progressionState.CurrentLevel}, " +
            $"Exp={progressionState.CurrentExp}/" +
            $"{progressionState.ExpToNextLevel}");
    }
}