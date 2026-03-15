using UnityEngine;
using System;

// 俯视角角色移动 + 碰陷阱Tag死亡销毁
public class TopDownPlayerMovement : MonoBehaviour
{
    #region 移动相关设置
    [Header("移动参数")]
    [Tooltip("角色移动速度")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("输入死区（避免微小输入触发移动）")]
    [SerializeField] private float inputDeadZone = 0.1f;

    [Header("组件引用")]
    [Tooltip("角色的精灵渲染器（用于翻转，可不填）")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    // 移动缓存变量
    private Vector2 velocity;          // 移动速度向量
    private float horizontalInput;     // 水平输入（A/D）
    private float verticalInput;       // 垂直输入（W/S）
    private float originalScaleX;      // 角色初始X轴缩放（用于翻转）
    #endregion

    [SerializeField] private Rigidbody2D rb;

    #region 死亡/碰撞核心设置
    [Header("死亡/碰撞设置")]
    [Tooltip("角色是否死亡（碰陷阱后变为true）")]
    public bool dead = false;
    [Tooltip("陷阱物体的Tag名称（必须和场景中陷阱的Tag完全一致）")]
    public string trapTag = "Trap"; // 重点：改这里为你实际的陷阱Tag！
    [Tooltip("死亡后延迟销毁的时间（秒）")]
    [SerializeField] private float destroyDelay = 0.1f; // 可在Inspector调整延迟时间
    #endregion



    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

    }
    #region 初始化
    private void Start()
    {
        // 保存角色初始X轴缩放（避免翻转时缩放异常）
        originalScaleX = Mathf.Abs(transform.localScale.x);
    }
    #endregion

    void Update()
    {
        UpdateAnimator();
    }
    private void UpdateAnimator()
    {
        if (animator == null) return;
        bool isMoving = velocity.sqrMagnitude > 0.0001f;
        //Debug.Log("Velocity is:" + velocity.ToString());
        Debug.Log("Is moving:" + isMoving);
        animator.SetBool("IsMoving", isMoving);
    }
    #region 移动逻辑（每帧执行，死亡后停止）
    private void FixedUpdate()
    {
        // 核心：角色死亡后，直接停止所有移动逻辑
        if (dead) return;

        GetWASDMovementInput();   // 获取WASD输入
        HandleGroundMovement();   // 计算移动速度
        ApplyMovement();          // 应用移动到角色位置
        HandleObjectFlip();       // 左右翻转（根据移动方向）
    }

    /// <summary>
    /// 仅获取WASD按键输入（不响应方向键）
    /// </summary>
    private void GetWASDMovementInput()
    {
        // 初始化输入为0
        horizontalInput = 0f;
        verticalInput = 0f;

        // W键：上
        if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
        // S键：下
        if (Input.GetKey(KeyCode.S)) verticalInput = -1f;
        // A键：左
        if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
        // D键：右
        if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;

        // 归一化输入向量（避免斜向移动速度过快）
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput).normalized;
        horizontalInput = inputVector.x;
        verticalInput = inputVector.y;
    }

    /// <summary>
    /// 计算俯视角移动速度
    /// </summary>
    private void HandleGroundMovement()
    {
        if (Mathf.Abs(horizontalInput) > inputDeadZone || Mathf.Abs(verticalInput) > inputDeadZone)
        {
            velocity.x = horizontalInput * moveSpeed;
            velocity.y = verticalInput * moveSpeed;
        }
        else
        {
            // 无有效输入时停止移动
            velocity.x = 0f;
            velocity.y = 0f;
        }
    }

    /// <summary>
    /// 应用移动到角色位置
    /// </summary>
    private void ApplyMovement()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        //transform.Translate(velocity * Time.fixedDeltaTime);
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }
    /// <summary>
    /// 角色左右翻转（修改Scale，非Sprite Flip）
    /// </summary>
    private void HandleObjectFlip()
    {
        if (Mathf.Abs(horizontalInput) > inputDeadZone)
        {
            Vector3 newScale = transform.localScale;
            // 右移（D）：X轴正缩放；左移（A）：X轴负缩放
            newScale.x = originalScaleX * Mathf.Sign(horizontalInput);
            transform.localScale = newScale;
        }
    }
    #endregion

    #region 碰撞死亡逻辑（核心：角色自身Collider2D碰陷阱Tag触发）
    /// <summary>
    /// 角色自己的Collider2D碰到其他物体时触发（仅触发器碰撞）
    /// 进入此方法 = 角色自己的Collider2D已经碰到东西了，只需判断是不是陷阱Tag
    /// </summary>
    /// <param name="other">被角色碰到的“对方物体”的Collider2D</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 已死亡则跳过，避免重复执行
        if (dead) return;

        // 关键判断：角色自己的Collider2D碰到的这个“对方”，是不是陷阱Tag？
        // bool isHitTrap = other.gameObject.CompareTag(trapTag);

        // if (isHitTrap)
        // {
        //     //dead = true; // 立即标记死亡，让场景控制器检测到
        //     // 延迟destroyDelay秒后执行销毁方法（默认0.1秒）
        //     //Invoke(nameof(DestroySelf), destroyDelay);
        // }
    }

    public void SetDead(bool isDead)
    {
        dead = isDead;
    }

    /// <summary>
    /// 延迟销毁自身的方法（供Invoke调用）
    /// </summary>
    private void DestroySelf()
    {
        Destroy(gameObject);
    }
    #endregion

    #region 辅助：自动赋值组件（新手友好）
    private void OnValidate()
    {
        // 自动给精灵渲染器赋值（如果没手动填）
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    #endregion
}