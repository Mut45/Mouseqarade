using System;
using System.Collections;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(PlayerRoleState))]
public class PlayerTauntState : NetworkBehaviour
{
    public event Action<bool> OnTauntStateChanged;

    public NetworkVariable<bool> isTaunting = new (
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private float tauntDuration = 3.0f;

    private Coroutine tauntCoroutine;

    public override void OnNetworkSpawn()
    {
        isTaunting.OnValueChanged += HandleTauntStateChanged;
        OnTauntStateChanged?.Invoke(isTaunting.Value);
    }

    public override void OnNetworkDespawn()
    {
        isTaunting.OnValueChanged -= HandleTauntStateChanged;
        OnTauntStateChanged?.Invoke(isTaunting.Value);
    }

    private void HandleTauntStateChanged(bool prevValue, bool currValue)
    {
        OnTauntStateChanged?.Invoke(currValue);
    }

    public bool GetIsTaunting()
    {
        return isTaunting.Value;
    }

    public void TryStartTaunt()
    {
        if (IsServer)
        {
            StartTauntFromServer();
        }
        else
        {
            StartTauntViaServerRPC();
        }
    }

    [ServerRpc]
    private void StartTauntViaServerRPC()
    {
        StartTauntFromServer();
    }

    private void StartTauntFromServer()
    {
        if (!IsServer) return;

        if (isTaunting.Value) return;

        if (roleState != null && roleState.GetRole() != PlayerRole.Mouse) return;

        isTaunting.Value = true;

        if (tauntCoroutine != null)
        {
            StopCoroutine(tauntCoroutine);
        }

        tauntCoroutine = StartCoroutine(TauntCorotuine());

    }

    private IEnumerator TauntCorotuine()
    {
        yield return new WaitForSeconds(tauntDuration);
        
        if (IsServer)
        {
            isTaunting.Value = false;
        }

        tauntCoroutine = null;
    }
    
    
}