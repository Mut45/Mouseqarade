using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Rigidbody2D))]
public class NetworkNPCController : NetworkBehaviour
{
    [SerializeField] private NPCRole[] roleList;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float changeTimer; // how long until change direction
    [SerializeField] private float movementSpeedMultiplier = 1f;
    private NetworkVariable<int> syncedRoleId = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<Vector2> syncedDir = new(
        Vector2.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<bool> syncedPaused = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server

    );
    private Vector2 cachedDir;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();                                                         
    }

    public override void OnNetworkSpawn()
    {
        syncedRoleId.OnValueChanged += OnRoleIdChange;
        syncedDir.OnValueChanged += OnDirectionChange;
        syncedPaused.OnValueChanged += OnPausedStatusChange;

        NetworkNPCManager.Instance?.Register(this);
        if (roleList != null) ApplyRoleVisuals();
        UpdateVisualState();
    }

    public override void OnNetworkDespawn()
    {
        syncedRoleId.OnValueChanged -= OnRoleIdChange;
        syncedDir.OnValueChanged -= OnDirectionChange;
        syncedPaused.OnValueChanged -= OnPausedStatusChange;

        NetworkNPCManager.Instance?.Unregister(this);
    }

    private void Update()
    {
        if (!IsServer) return;
        NPCRole role = GetCurrentRole();
        if (role == null || syncedPaused.Value) return;

        changeTimer -= Time.deltaTime;
        if (changeTimer <= 0f)
            PickNewDirection(false);
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        NPCRole role = GetCurrentRole();
        if (role == null || syncedPaused.Value) return;
        
        Vector2 currentDir = syncedDir.Value;
        float moveSpeed = role.moveSpeed * movementSpeedMultiplier;
        
        Vector2 newPos = rb.position + currentDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;
        NPCRole role = GetCurrentRole();
        if (role == null || syncedPaused.Value) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Bounds"))
        {
            PickNewDirection(force: true);
        }
    }

    private NPCRole GetCurrentRole()
    {
        int index = syncedRoleId.Value;
        if (roleList == null || index < 0 || index >= roleList.Length) return null;
        return roleList[index];
    }

    #region Exposed functions to modify movement speed
    public void SetMovementSpeedMultiplier(float multiplier)
    {
        movementSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public float GetCurrentMoveSpeed()
    {
        NPCRole role = GetCurrentRole();
        if (role == null) return 0f;

        return role.moveSpeed * movementSpeedMultiplier;
    }

    #endregion

    #region NetworkVariable On Change
    private void OnRoleIdChange(int oldValue, int newValue)
    {
        ApplyRoleVisuals();
        UpdateVisualState();
    }

    private void OnDirectionChange(Vector2 oldValue, Vector2 newValue)
    {
        UpdateVisualState();
    }

    private void OnPausedStatusChange(bool oldValue, bool newValue)
    {
        UpdateVisualState();
    }
    #endregion

    #region Visual State Update
    private void UpdateVisualState()
    {
        UpdateAnimator();
        UpdateSpriteFlip();    
    }
    private void UpdateSpriteFlip()
    {
        if (spriteRenderer == null) return;
        Vector2 currentDir = syncedDir.Value;
        if (currentDir.x > 0.01f)
            spriteRenderer.flipX = false; 
        else if (currentDir.x < -0.01f)
            spriteRenderer.flipX = true; 
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;
        Vector2 currentDir = syncedDir.Value;
        animator.SetBool("IsMoving", currentDir.sqrMagnitude > 0.0001f);
    }
    #endregion

    #region Role related
    public NPCRole GetRole()
    {
        return roleList[syncedRoleId.Value];
    }
    public void SetRoleById(int id)
    {
        if (!IsServer) return;
        if (roleList == null || id < 0 || id >= roleList.Length) return;

        syncedRoleId.Value = id;
        PickNewDirection(force: true);
    }
    private void ApplyRoleVisuals()
    {
        NPCRole role = GetCurrentRole();
        if (role == null) return;

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
    }

    #endregion

    #region Pause/Resume
    public void PauseMovement()
    {
        if (!IsServer) return;
        if (syncedPaused.Value) return;

        cachedDir = syncedDir.Value;
        syncedDir.Value = Vector2.zero;
        syncedPaused.Value = true;
    }

    public void ResumeMovement()
    {
        if (!IsServer) return;
        if (!syncedPaused.Value) return;

        syncedPaused.Value = false;
        syncedDir.Value = cachedDir;
    }
    #endregion

    #region NPC movement logic
    private AxisRule GetCurrentMovementRule()
    {
        NPCRole role = GetCurrentRole();
        if (role == null) return AxisRule.FourWay;
        return role.movementRule;
    }
    private void PickNewDirection(bool force)
    {
        NPCRole role = GetCurrentRole();
        if (role == null) return;

        changeTimer = Random.Range(role.minWanderTime, role.maxWanderTime);

        if (!force && Random.value < role.idleChance)
        {
            syncedDir.Value = Vector2.zero;
            return;
        }
        syncedDir.Value = GetRandomDirection(role.movementRule);
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
    #endregion
}