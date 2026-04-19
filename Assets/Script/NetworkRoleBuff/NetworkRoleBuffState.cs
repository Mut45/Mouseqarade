using System;
using Unity.Netcode;
using UnityEngine;


public enum BuffCardEffectId
{
    CatMoveSpeed_Permanent = 0,
    CatMoveSpeed_OnFail_Temporary = 1,
    DisableClock = 2,
    RedLightGreenLight = 3,
    MouseProximitySound = 4,
    MouseInteractionSound = 5,
}

[RequireComponent(typeof(NetworkObject))]
public class NetworkRoleBuffState : NetworkBehaviour
{
    public static NetworkRoleBuffState Instance { get; private set; }

    [SerializeField] private NetworkList<int> catOwnedBuffs = new();

    [SerializeField] private NetworkList<int> mouseOwnedBuffs = new();

    // 1. Cat temporary speed buff card effect
    private NetworkVariable<bool> catTempSpeedActive = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<double> catTempSpeedEndTime = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // 2. Disable clock buff card effect
    [SerializeField] private NetworkVariable<bool> isClockDisabled = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // 3. Red light green light buff card effect
    [SerializeField] private NetworkVariable<bool> isRedLightGreenLightActive = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<double> redLightGreenLightEndTime = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    #region OnChange Actions for buff card effects
    public event Action OnCatBuffsChanged;
    public event Action OnMouseBuffsChanged;
    public event Action OnRuntimeStateChanged;
    #endregion

    #region Getters for network variables
    public bool IsCatTempSpeedActive => catTempSpeedActive.Value;
    public double CatTempSpeedEndTime => catTempSpeedEndTime.Value;
    public bool IsClockDisabled => isClockDisabled.Value;

    public bool IsRedLightGreenLightActive => isRedLightGreenLightActive.Value;
    public double RedLightGreenLightEndTime => redLightGreenLightEndTime.Value;
    #endregion

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
        catOwnedBuffs.OnListChanged += HandleCatBuffsListChanged;
        mouseOwnedBuffs.OnListChanged += HandleMouseBuffsListChanged;

        catTempSpeedActive.OnValueChanged += HandleRuntimeBoolChanged;
        catTempSpeedEndTime.OnValueChanged += HandleRuntimeDoubleChanged;
        isClockDisabled.OnValueChanged += HandleRuntimeBoolChanged;
        isRedLightGreenLightActive.OnValueChanged += HandleRuntimeBoolChanged;
        redLightGreenLightEndTime.OnValueChanged += HandleRuntimeDoubleChanged;

        OnCatBuffsChanged?.Invoke();
        OnMouseBuffsChanged?.Invoke();
        OnRuntimeStateChanged?.Invoke();
    }

    #region Handlers for buff card effects state changes
    private void HandleRuntimeBoolChanged(bool oldValue, bool newValue)
    {
        OnRuntimeStateChanged?.Invoke();
    }

    private void HandleRuntimeDoubleChanged(double oldValue, double newValue)
    {
        OnRuntimeStateChanged?.Invoke();
    }

    private void HandleCatBuffsListChanged(NetworkListEvent<int> changeEvent)
    {
        OnCatBuffsChanged?.Invoke();
    }

    private void HandleMouseBuffsListChanged(NetworkListEvent<int> changeEvent)
    {
        OnMouseBuffsChanged?.Invoke();
    }
    #endregion

    #region Server-side functions to add/remove buff card effects for role
    public void AddBuffForRoleServer(PlayerRole role, BuffCardEffectId buff)
    {
        if (!IsServer) return;

        NetworkList<int> list = GetBuffList(role);

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == (int)buff)
                return;
        }

        list.Add((int)buff);
    }

    public void ClearBuffsForRoleServer(PlayerRole role)
    {
        if (!IsServer) return;
        GetBuffList(role).Clear();
    }

    #endregion
    
    #region Exposed public functions to set and check buff effects
    public void SetCatTempSpeedFromServer(bool active, double endTime)
    {
        if (!IsServer) return;

        catTempSpeedActive.Value = active;
        catTempSpeedEndTime.Value = active ? endTime : 0;
    }

    public void SetClockDisabledFromServer(bool disabled)
    {
        if (!IsServer) return;

        isClockDisabled.Value = disabled;
    }

    public void SetRedLightGreenLightFromServer(bool active, double endTime)
    {
        if (!IsServer) return;

        isRedLightGreenLightActive.Value = active;
        redLightGreenLightEndTime.Value = active ? endTime : 0;
    }

    public bool HasBuffForRole(PlayerRole role, BuffCardEffectId buff)
    {
        NetworkList<int> list = GetBuffList(role);

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == (int)buff)
                return true;
        }

        return false;
    }
    #endregion

    private NetworkList<int> GetBuffList(PlayerRole role)
    {
        return role == PlayerRole.Cat ? catOwnedBuffs : mouseOwnedBuffs;
    }

}