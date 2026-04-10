using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerRoleApplier : MonoBehaviour
{
    [Header("Gameplay & Visual State of Player")]
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private PlayerDisguiseState disguiseState;

    [Header("Visual Component References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Role Settings")]
    [SerializeField] private float mouseMoveSpeed = 5.0f;
    [SerializeField] private float catMoveSpeed = 5.0f;

    [Header("Base Visuals")]
    [SerializeField] private RuntimeAnimatorController mouseAnimationController;
    [SerializeField] private RuntimeAnimatorController catAnimationController;
    private void OnEnable()
    {
        if (roleState != null) 
        {
            roleState.OnRoleChanged += ApplyPlayerRole;
        }
    }

    private void OnDisable()
    {
        if (roleState != null)
        {
            roleState.OnRoleChanged -= ApplyPlayerRole;
        }
    }

    private void Start()
    {
        if (roleState != null)
        {
            ApplyPlayerRole(roleState.GetRole());
        }
    }
    
    #region Testing for role setting
    [ContextMenu("Test/Set Mouse Role")]
    private void TestSetMouseRole()
    {
        if (roleState == null)
        {
            Debug.LogWarning("PlayerRoleState is missing.");
            return;
        }

        roleState.SetRole(PlayerRole.Mouse);
    }

    [ContextMenu("Test/Set Cat Role")]
    private void TestSetCatRole()
    {
        if (roleState == null)
        {
            Debug.LogWarning("PlayerRoleState is missing.");
            return;
        }

        roleState.SetRole(PlayerRole.Cat);
    }

    [ContextMenu("Test/Reapply Current Role")]
    private void TestReapplyCurrentRole()
    {
        if (roleState == null)
        {
            Debug.LogWarning("PlayerRoleState is missing.");
            return;
        }

        ApplyPlayerRole(roleState.GetRole());
    }
    #endregion

    private void ApplyPlayerRole(PlayerRole role)
    {
        switch (role)
        {
            case PlayerRole.Mouse:
                ApplyMouseRole();
                break;
            case PlayerRole.Cat:
                ApplyCatRole();
                break;
        }
    }
    private void ApplyMouseRole()
    {
        if (movement != null)
        {
            movement.SetMovementSpeed(mouseMoveSpeed);
        }
        
        if (animator != null && mouseAnimationController != null)
        {
            animator.enabled = true;
            animator.runtimeAnimatorController = mouseAnimationController;
        }

    }

    private void ApplyCatRole()
    {
        if (movement != null)
        {
            movement.SetMovementSpeed(catMoveSpeed);            
        }

        if (animator != null && catAnimationController != null)
        {
            animator.enabled = true;
            animator.runtimeAnimatorController = catAnimationController;
        }

        if (disguiseState != null && disguiseState.GetIsDisguised())
        {
            disguiseState.ClearDisguise();
        }
    }


}