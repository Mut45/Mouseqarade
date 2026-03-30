using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerVisualController : NetworkBehaviour
{
    [Header("Cat")]
    [SerializeField] private RuntimeAnimatorController catAnimatorController;
    [SerializeField] private Sprite catSprite;
    [SerializeField] private float catDefaultMoveSpeed = 5.0f;

    [Header("Mouse")]
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private PlayerDisguiseApplier roleApplier;

    private void ApplyCatDefaultVisual()
    {
        
    }

    private void ApplyMouseDefaultVisual()
    {
        
    }

    private void ApplyMouseDisguisedVisual(NPCRole targetNPC)
    {
        if (roleApplier == null || targetNPC == null) return;
        //roleApplier.ApplyRole(targetNPC);
    }

    private void UpdateVisuals()
    {
        
    }


}