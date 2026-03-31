using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool ifMovementLocked;
    private Vector2 currentMoveInput;
    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (animator == null) animator = GetComponent<Animator>();

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void SetMovementSpeed(float speed)
    {
        moveSpeed = speed;
    }
    public void SetMovementInput(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude > 1f) moveInput = moveInput.normalized;

        currentMoveInput = moveInput;
    }
    public void SetMovementLocked(bool ifLocked)
    {
        ifMovementLocked = ifLocked;

        if (ifLocked)
        {
            currentMoveInput = Vector2.zero;
            UpdateVisuals(Vector2.zero);
        }
    }

    public void Move(float deltaTime)
    {
        if (ifMovementLocked) return;

        Vector2 newPosition =  rb.position + currentMoveInput * moveSpeed * deltaTime;
        rb.MovePosition(newPosition);

        UpdateVisuals(currentMoveInput);

    }
    public void UpdateVisuals(Vector2 moveInput)
    {
        if (animator != null) animator.SetBool("IsMoving", moveInput.sqrMagnitude > 0.001f);

        if (spriteRenderer != null && Mathf.Abs(moveInput.x) > 0.01f) spriteRenderer.flipX = moveInput.x < 0f;

    }

}
