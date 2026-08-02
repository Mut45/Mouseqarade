using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CatPassiveExpSource : NetworkBehaviour
{
    [Min(0.1f)]
    [SerializeField] private float rewardIntervalSeconds = 10f;

    private bool passiveExpActive;
    private Coroutine rewardRoutine;

    public override void OnNetworkDespawn()
    {
        StopPassiveExpFromServer();
    }

    public void SetPassiveExpActiveFromServer(bool active)
    {
        if (!IsServer)
            return;

        if (passiveExpActive == active)
            return;

        passiveExpActive = active;

        if (active)
        {
            rewardRoutine = StartCoroutine(PassiveRewardRoutine());
        }
        else
        {
            StopPassiveExpFromServer();
        }
    }

    private IEnumerator PassiveRewardRoutine()
    {
        var wait = new WaitForSeconds(rewardIntervalSeconds);

        while (passiveExpActive)
        {
            yield return wait;

            if (!passiveExpActive)
                break;

            if (!NetworkLookUp.TryGetClientIdByRole(
                    NetworkManager,
                    PlayerRole.Cat,
                    out ulong catClientId))
            {
                continue;
            }

            CatExpRewardEvents.PublishFromServer(
                new CatExpRewardEvent(
                    CatExpRewardEventType.PassiveTime,
                    NetworkObjectId,
                    catClientId));
        }

        rewardRoutine = null;
    }

    private void StopPassiveExpFromServer()
    {
        passiveExpActive = false;

        if (rewardRoutine != null)
        {
            StopCoroutine(rewardRoutine);
            rewardRoutine = null;
        }
    }
}