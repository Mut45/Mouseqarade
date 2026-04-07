using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerDisguiseApplier : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private PlayerDisguiseState disguiseState;

    [Header("Default Mouse Movement & Visual")]
    [SerializeField] private float defaultMouseMoveSpeed = 5.0f;
    [SerializeField] private RuntimeAnimatorController defaultMouseAnimatorController;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (roleState == null) roleState = GetComponent<PlayerRoleState>();
        if (disguiseState == null) disguiseState = GetComponent<PlayerDisguiseState>();

    }
    private void OnEnable()
    {
        if (disguiseState != null)
        {
            disguiseState.OnDisguiseChanged += HandleOnDisguiseChanged;
        }

        if (roleState != null)
        {
            roleState.OnRoleChanged += HandleRoleChanged;            
        }
    }

    private void OnDisable()
    {
        if (disguiseState != null)
        {
            disguiseState.OnDisguiseChanged -= HandleOnDisguiseChanged;
        }

        if (roleState != null)
        {
            roleState.OnRoleChanged -= HandleRoleChanged;
        }
    }

    void Start()
    {
        UpdateDisguiseApplication();
    }

    private void HandleRoleChanged(PlayerRole playerRole)
    {
        UpdateDisguiseApplication();
    }

    private void HandleOnDisguiseChanged(bool isDisguised, ulong npcNOId)
    {
        UpdateDisguiseApplication();
    }

    private void ApplyDisguiseFromNpcRole(NPCRole npcRole)
    {
        if (movement != null)
        {
            movement.SetMovementSpeed(npcRole.moveSpeed);
        }

        if (animator != null && npcRole.animatorOverride != null)
        {
            animator.enabled = true;
            animator.runtimeAnimatorController = npcRole.animatorOverride;
            return;
        }
        if (animator != null && npcRole.animatorOverride == null)
        {
            RestoreToDefaultMouseForm();
        }

    }

    private void UpdateDisguiseApplication()
    {
        if (roleState == null || disguiseState == null) return;

        if (roleState.GetRole() != PlayerRole.Mouse) return; // early out if the player is cat

        if (!disguiseState.GetIsDisguised())
        {
            RestoreToDefaultMouseForm();
            return;
        }

        ulong npcNOId = disguiseState.GetTargetedNPCNOId();
        
        if (!NetworkLookUp.TryGetNetworkNpc(NetworkManager.Singleton, npcNOId, out NetworkNPCController npc) || npc == null)
        {
            RestoreToDefaultMouseForm();
            return;
        }
        NPCRole npcRole = npc.GetRole();
        if (npcRole == null)
        {
            RestoreToDefaultMouseForm();
            return;
        }

        ApplyDisguiseFromNpcRole(npcRole);
    }

    private void RestoreToDefaultMouseForm()
    {
        if (roleState != null && roleState.GetRole() != PlayerRole.Mouse)
        {
            return;
        }

        if (movement != null) movement.SetMovementSpeed(defaultMouseMoveSpeed);

        if (animator != null && defaultMouseAnimatorController != null)
        {
            animator.enabled = true;
            animator.runtimeAnimatorController = defaultMouseAnimatorController;
        }
    }
}
