using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoleApplier : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TopDownPlayerMovement movement; // your movement script (must expose SetSpeed)

    private RuntimeAnimatorController baseController;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (movement == null) movement = GetComponent<TopDownPlayerMovement>();

        if (animator != null)
            baseController = animator.runtimeAnimatorController;
    }

    public void ApplyRole(NPCRole role)
    {
        if (role == null) return;

        if (movement != null)
            movement.SetSpeed(role.moveSpeed);

        if (animator != null && role.animatorOverride != null)
        {
            animator.runtimeAnimatorController = role.animatorOverride;
            animator.enabled = true;
        }
        else if (spriteRenderer != null && role.idleSprite != null)
        {
            spriteRenderer.sprite = role.idleSprite;
            if (animator != null) animator.enabled = false;
        }
        else
        {
            if (animator != null && baseController != null)
                animator.runtimeAnimatorController = baseController;
    }
}
}
