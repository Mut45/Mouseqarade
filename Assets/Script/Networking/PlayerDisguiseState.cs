using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(PlayerDisguiseApplier))]
[RequireComponent(typeof(PlayerRoleState))]
public class PlayerDisguiseState : NetworkBehaviour
{
    private NetworkVariable<bool> isDisguised = new(
        false,
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<ulong> targetedNPCNOId = new (
        0,
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    ); // if the NetworkObject Id is 0, the default mouse appearance is applied
    
    public event Action<bool, ulong> OnDisguiseChanged;
    public override void OnNetworkSpawn()
    {
        isDisguised.OnValueChanged += HandleIsDisguisedChanged;
        targetedNPCNOId.OnValueChanged += HandleTargetedNPCIdChanged;

        OnDisguiseChanged?.Invoke(isDisguised.Value, targetedNPCNOId.Value);
    }

    public override void OnNetworkDespawn()
    {
        isDisguised.OnValueChanged -= HandleIsDisguisedChanged;
        targetedNPCNOId.OnValueChanged -= HandleTargetedNPCIdChanged;
    }

    #region Disguise Change Handler
    private void HandleIsDisguisedChanged(bool oldValue, bool newValue)
    {
        OnDisguiseChanged?.Invoke(isDisguised.Value, targetedNPCNOId.Value);
    }

    private void HandleTargetedNPCIdChanged(ulong oldValue, ulong newValue)
    {
        OnDisguiseChanged?.Invoke(isDisguised.Value, targetedNPCNOId.Value);
    }
    #endregion

    #region Getter & Setter for Disguise state
    public ulong GetTargetedNPCNOId()
    {
        return targetedNPCNOId.Value;
    }

    public bool GetIsDisguised()
    {
        return isDisguised.Value;
    }

    public void SetDisguise(bool disguised, ulong npcNetworkObjectId)
    {
        if (IsServer)
        {
            SetDisguiseFromServer(disguised, npcNetworkObjectId);
        }
        else
        {
            SetDisguiseViaServerRpc(disguised, npcNetworkObjectId);
        }
    }

    #endregion

    public void ClearDisguise()
    {
        SetDisguise(false, 0);
    }

    [ServerRpc]
    private void SetDisguiseViaServerRpc(bool disguised, ulong npcNetworkObjectId)
    {
        SetDisguiseFromServer(disguised, npcNetworkObjectId);
    }
    private void SetDisguiseFromServer(bool disguised, ulong npcNetworkObjectId)
    {
        isDisguised.Value = disguised;
        targetedNPCNOId.Value = disguised ? npcNetworkObjectId : 0;
    }
    // public void ApplyDisguieFromNpc(NetworkNPCController npc)
    // {
    //     if (!IsServer) return;
    //     if (npc == null) return;
    //     IsDisguised.Value = true;
    //     TargetedNPCNOId.Value = npc.NetworkObjectId;
    // }

    // public void CancelDisguise()
    // {
    //     if (!IsServer) return;

    //     IsDisguised.Value = false;
    //     TargetedNPCNOId.Value = 0;
    // }



}