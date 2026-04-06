using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkRoleBuffState))]
public class NetworkRoleBuffSystem : NetworkBehaviour
{
    [Header("Buff State Reference")]
    [SerializeField] NetworkRoleBuffState roleBuffState;
    
    [Header("Buff effect parameters config")]
    [SerializeField] private float catTempSpeedDuration = 5.0f;
    [SerializeField] private float redLightGreenLightDelay = 5.0f;
    [SerializeField] private float redLightGreenLightDuration = 3.0f;

    [Header("Clock object reference")]
    [SerializeField] private NetworkClockController clockController;

    [Header("Registered cat primary action controller")]
    private readonly HashSet<CatPrimaryActionController> registeredCatControllers = new();

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

        if (roleBuffState == null) roleBuffState = GetComponent<NetworkRoleBuffState>();
    }

    // #region Registering the cat player's primary action controller in the scene
    // public void RegisterCatController(CatPrimaryActionController controller)
    // {
    //     if (!IsServer) return;
    //     if (controller == null) return;
    //     if (!registeredCatControllers.Add(controller)) return;

    //     controller.OnFailedCatchNpc += HandleFailedCatchNpc;
    // }

    // public void UnregisterCatController(CatPrimaryActionController controller)
    // {
    //     if (controller == null) return;
    //     if (!registeredCatControllers.Remove(controller)) return;

    //     controller.OnFailedCatchNpc -= HandleFailedCatchNpc;
    // }

    // private void HandleFailedCatchNpc(CatPrimaryActionController source)
    // {
    //     if (!IsServer) return;
    //     if (source == null) return;
    //     if (NetworkRoleBuffState.Instance == null) return;

    //     PlayerRoleState roleState = source.GetComponent<PlayerRoleState>();
    //     if (roleState == null || roleState.GetRole() != PlayerRole.Cat) return;

    //     if (!NetworkRoleBuffState.Instance.HasBuffForRole(
    //         PlayerRole.Cat,
    //         BuffCardEffectId.CatMoveSpeed_OnFail_Temporary))
    //     {
    //         return;
    //     }

    //     double endTime = NetworkManager.Singleton.ServerTime.Time + catTempSpeedDuration;
    //     NetworkRoleBuffState.Instance.SetCatTempSpeedFromServer(true, endTime);
    // }
    // #endregion

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

        if (roleBuffState == null) return;

        roleBuffState.AddBuffForRoleServer(role, effectId);

        switch (effectId)
        {
            case BuffCardEffectId.DisableClock:
                roleBuffState.SetClockDisabledFromServer(true);
                break;

            case BuffCardEffectId.RedLightGreenLight:
                StartRedLightGreenLightFromServer();
                break;
        }
    }
    #endregion

    #region Red light green light state change sequence
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

        roleBuffState.SetRedLightGreenLightFromServer(true, endTime);

        yield return new WaitForSeconds(redLightGreenLightDelay + redLightGreenLightDuration);

        roleBuffState.SetRedLightGreenLightFromServer(false, 0);
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

        if (roleBuffState == null) return;

        if (!roleBuffState.HasBuffForRole(PlayerRole.Cat, BuffCardEffectId.CatMoveSpeed_OnFail_Temporary)) return;
    
        if (catTempSpeedRoutine != null)
        {
            StopCoroutine(catTempSpeedRoutine);
        }

        catTempSpeedRoutine = StartCoroutine(CatTempSpeedRoutine());
    }

    private IEnumerator CatTempSpeedRoutine()
    {
        double endTime = NetworkManager.ServerTime.Time + catTempSpeedDuration;
        roleBuffState.SetCatTempSpeedFromServer(true, endTime);

        yield return new WaitForSeconds(catTempSpeedDuration);

        roleBuffState.SetCatTempSpeedFromServer(false, 0);
        catTempSpeedRoutine = null;
    }
    #endregion

}