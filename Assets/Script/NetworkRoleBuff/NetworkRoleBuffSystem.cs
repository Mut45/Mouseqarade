using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkRoleBuffState))]
public class NetworkRoleBuffSystem : NetworkBehaviour
{
    [Header("Buff effect parameters config")]
    [SerializeField] private float catTempSpeedDuration = 5.0f;
    [SerializeField] private float redLightGreenLightDelay = 5.0f;
    [SerializeField] private float redLightGreenLightDuration = 3.0f;

    [Header("Clock object reference")]
    [SerializeField] private NetworkClockController clockController;

    [Header("Debug Test")]
    [SerializeField] private PlayerRole debugRole = PlayerRole.Cat;
    [SerializeField] private BuffCardEffectId debugEffectId = BuffCardEffectId.CatMoveSpeed_Permanent;

    private Coroutine catTempSpeedRoutine;
    private Coroutine redLightGreenLightRoutine;

    public static NetworkRoleBuffSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    #region Acquire the buff effect for role
    public void AcquireEffectForRole(PlayerRole role, BuffCardEffectId effectId)
    {
        if (IsServer)
        {
            AcquireEffectForRoleFromServer(role, effectId);
            return;
        }

        AcquireEffectForRoleViaServerRpc(role, effectId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void AcquireEffectForRoleViaServerRpc(PlayerRole role, BuffCardEffectId effectId)
    {
        AcquireEffectForRoleFromServer(role, effectId);
    }

    private void AcquireEffectForRoleFromServer(PlayerRole role, BuffCardEffectId effectId)
    {
        if (!IsServer) return;
        if (NetworkRoleBuffState.Instance == null) return;
        NetworkRoleBuffState.Instance.AddBuffForRoleServer(role, effectId);
        switch (effectId)
        {
            case BuffCardEffectId.DisableClock:
                NetworkRoleBuffState.Instance.SetClockDisabledFromServer(true);
                break;

            // case BuffCardEffectId.RedLightGreenLight:
            //     StartRedLightGreenLightFromServer();
            //     break;
        }
    }
    #endregion

    #region Red light green light state change sequence
    public void TriggerRedLightGreenLight()
    {
        if (IsServer)
        {
            TriggerRedLightGreenLightFromServer();
            return;
        }

        TriggerRedLightGreenLightViaServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TriggerRedLightGreenLightViaServerRpc()
    {
        TriggerRedLightGreenLightFromServer();
    }

    private void TriggerRedLightGreenLightFromServer()
    {
        if (!IsServer) return;
        if (NetworkRoleBuffState.Instance == null) return;

        bool hasBuff = NetworkRoleBuffState.Instance.HasBuffForRole(
        PlayerRole.Cat,
        BuffCardEffectId.RedLightGreenLight);
        Debug.Log($"[NetworkRoleBuffSystem] Trigger RLGL requested. Cat has buff: {hasBuff}");
        
        if (!NetworkRoleBuffState.Instance.HasBuffForRole(
            PlayerRole.Cat,
            BuffCardEffectId.RedLightGreenLight))
        {
            return;
        }

        StartRedLightGreenLightFromServer();
    }

    private void StartRedLightGreenLightFromServer()
    {
        if (!IsServer) return;

        if (redLightGreenLightRoutine != null)
        {
            StopCoroutine(redLightGreenLightRoutine);
        }

        redLightGreenLightRoutine = StartCoroutine(RedLightGreenLightRoutine());
    }

    private IEnumerator RedLightGreenLightRoutine()
    {
        double startTime = NetworkManager.ServerTime.Time + redLightGreenLightDelay;
        double endTime = startTime + redLightGreenLightDuration;

        NetworkRoleBuffState.Instance.SetRedLightGreenLightFromServer(true, endTime);

        yield return new WaitForSeconds(redLightGreenLightDelay);

        NetworkNPCManager.Instance?.PauseAndThenResume(redLightGreenLightDuration);

        yield return new WaitForSeconds(redLightGreenLightDuration);

        NetworkRoleBuffState.Instance.SetRedLightGreenLightFromServer(false, 0);
        redLightGreenLightRoutine = null;
    }
    #endregion

    #region Cat failed to catch mouse sequence
    public void NotifyCatFailedCatch()
    {
        if (!IsServer)
        {
            NotifyCatFailedCatchViaServerRpc();
            return;
        }
        NotifyCatFailedCatchFromServer();
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void NotifyCatFailedCatchViaServerRpc()
    {
        NotifyCatFailedCatchFromServer();
    }

    private void NotifyCatFailedCatchFromServer()
    {
        if (!IsServer) return;

        if (NetworkRoleBuffState.Instance == null) return;

        if (!NetworkRoleBuffState.Instance.HasBuffForRole(PlayerRole.Cat, BuffCardEffectId.CatMoveSpeed_OnFail_Temporary)) return;
    
        if (catTempSpeedRoutine != null)
        {
            StopCoroutine(catTempSpeedRoutine);
        }

        catTempSpeedRoutine = StartCoroutine(CatTempSpeedRoutine());
    }

    private IEnumerator CatTempSpeedRoutine()
    {
        double endTime = NetworkManager.ServerTime.Time + catTempSpeedDuration;
        NetworkRoleBuffState.Instance.SetCatTempSpeedFromServer(true, endTime);

        yield return new WaitForSeconds(catTempSpeedDuration);

        NetworkRoleBuffState.Instance.SetCatTempSpeedFromServer(false, 0);
        catTempSpeedRoutine = null;
    }
    #endregion

    #region Rat interaction sound
    public void NotifyMouseInteraction()
    {
        if (IsServer)
        {
            NotifyMouseInteractionFromServer();
            return;
        }

        NotifyMouseInteractionViaServerRpc();
    }
    private void NotifyMouseInteractionFromServer()
    {
        if (!IsServer) return;
        if (NetworkRoleBuffState.Instance == null) return;
        if (NetworkManager == null) return;

        bool catHasBuff = NetworkRoleBuffState.Instance.HasBuffForRole(
            PlayerRole.Cat,
            BuffCardEffectId.MouseInteractionSound);

        if (!catHasBuff)
            return;

        if (!NetworkLookUp.TryGetPlayerObjectByRole(NetworkManager, PlayerRole.Cat, out NetworkObject catPlayerObject))
            return;

        PlayerInteractionSoundController soundController =
            catPlayerObject.GetComponent<PlayerInteractionSoundController>();

        if (soundController == null)
            return;

        ulong catClientId = catPlayerObject.OwnerClientId;

        soundController.PlayInteractionSoundClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { catClientId }
            }
        });
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]    
    private void NotifyMouseInteractionViaServerRpc()
    {
        NotifyMouseInteractionFromServer();
    }
    #endregion
}