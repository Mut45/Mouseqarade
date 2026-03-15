using System.Collections;
using UnityEngine;

public class NPCController : MonoBehaviour
{
   [SerializeField]private NPCRole role;
    private Vector2 currentDir;
    private Vector2 cachedDir;
    [SerializeField]private float changeTimer; // how long until change direction
    [SerializeField]private Animator animator;
    [SerializeField]private SpriteRenderer spriteRenderer;
    
    private bool isPaused;
    private Rigidbody2D rb;

    private void OnDestroy()
    {
        NPCManager.Instance?.Unregister(this);
    }
    void Update()
    {
        if (role == null || isPaused) return;
        UpdateAnimator();
        UpdateSpriteFlip();
        changeTimer -= Time.deltaTime;
        if (changeTimer <= 0f)
            PickNewDirection(false);
    }

    void FixedUpdate()
    {
        if (role == null || isPaused) return;
        
        // rb = currentDir * role.moveSpeed;
        Vector3 velocity = currentDir * role.moveSpeed;
        transform.Translate(velocity * Time.fixedDeltaTime);
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        bool isMoving = currentDir.sqrMagnitude > 0.0001f;
        animator.SetBool("IsMoving", isMoving);
    }
    public void PauseMovement()
    {
        if (isPaused) return;
        animator.SetBool("IsMoving", false);
        isPaused = true;
        cachedDir = currentDir;
        currentDir = Vector2.zero;
    }
    public void ResumeMovement()
    {
        if (!isPaused) return;
        isPaused = false;
        animator.SetBool("IsMoving", true);
        currentDir = cachedDir;
    }
    public NPCRole GetRole()
    {
        return role;
    }
    public void SetRole(NPCRole newRole)
    {
        role = newRole;
        ApplyRoleVisuals();
        PickNewDirection(true);
    }
    private void ApplyRoleVisuals(){
        if (role == null) return;

        if (animator != null && role.animatorOverride != null)
        {
            animator.runtimeAnimatorController = role.animatorOverride;
            animator.enabled = true;
        }
        else if (spriteRenderer != null && role.idleSprite != null)
        {
            spriteRenderer.sprite = role.idleSprite;
            if (animator != null)
                animator.enabled = false;
        }
    }

    private AxisRule GetCurrentMovementRule()
    {
        if (role == null) return AxisRule.FourWay;
        return role.movementRule;
    }

    public void SetMovementRule(AxisRule newRule, bool repickDirection = true)
    {
        // movementRuleOverride = newRule;
        if (repickDirection)
            PickNewDirection(force: true);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Bounds"))
        {
            PickNewDirection(true);
        }
    }

    private void PickNewDirection(bool force)
    {
        if (role == null) return;
        changeTimer = Random.Range(role.minWanderTime, role.maxWanderTime);

        if (!force && Random.value < role.idleChance)
        {
            currentDir = Vector2.zero;
            return;
        }
        AxisRule rule = GetCurrentMovementRule();
        currentDir = GetRandomDirection(rule);
    }

    public void ClearMovementRuleOverride(bool repickDirection = true)
    {
        if (repickDirection)
            PickNewDirection(force: true);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        NPCManager.Instance?.Register(this);
        if (animator == null)
        {
             animator = GetComponentInChildren<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
    }
    private void UpdateSpriteFlip()
    {
        if (spriteRenderer == null) return;

        if (currentDir.x > 0.01f)
            spriteRenderer.flipX = false; 
        else if (currentDir.x < -0.01f)
            spriteRenderer.flipX = true; 
    }
    private void Start()
    {
        if (role == null)
        {
            enabled = false;
            return;
        }
        PickNewDirection(force: true);
    }

    private static Vector2 GetRandomDirection(AxisRule rule)
    {
        switch (rule)
        {
            case AxisRule.HorizontalOnly:
                return Random.value < 0.5f ? Vector2.left : Vector2.right;

            case AxisRule.VerticalOnly:
                return Random.value < 0.5f ? Vector2.up : Vector2.down;

            case AxisRule.EightWay:
            {
                Vector2[] dirs =
                {
                    Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                    (Vector2.up + Vector2.left).normalized,
                    (Vector2.up + Vector2.right).normalized,
                    (Vector2.down + Vector2.left).normalized,
                    (Vector2.down + Vector2.right).normalized
                };
                return dirs[Random.Range(0, dirs.Length)];
            }

            case AxisRule.FourWay:
            default:
            {
                Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
                return dirs[Random.Range(0, dirs.Length)];
            }
        }
    }
}