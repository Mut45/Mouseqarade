using System.Collections;
using UnityEngine;

public class CatPlayerMovement : MonoBehaviour
{

    [Header("�ƶ�����")]
    [SerializeField] private float moveSpeed = 2f; // �ƶ��ٶ�
    [SerializeField] private float inputDeadZone = 0.1f; // ��������
    private float originalScaleX;

    // �����������
    [Header("��������")]
    [SerializeField] private SpriteRenderer spriteRenderer; // ��ɫ����
    [SerializeField] private Animator animator; // ������������������أ�
    [SerializeField] private float attackCoolDown = 0.5f; // ������ȴ����ֹ���㣩
    // 新增：音效相关组件/资源
    [SerializeField] private AudioSource audioSource; // 播放音效的组件
    [SerializeField] private AudioClip attackSoundClip; // 攻击音效文件
    [SerializeField][Range(0f, 1f)] private float attackSoundVolume = 0.8f; // 音效音量
    private bool isAttacking = false; // �Ƿ����ڹ���
    private float lastAttackTime; // �ϴι���ʱ��

    // �ƶ����
    private Vector2 velocity; // �ƶ��ٶ�����
    private float horizontalInput; // ˮƽ���루��/�ҷ������
    private float verticalInput; // ��ֱ���루��/�·������

    // ����Trigger���ƣ���Animator�����������һ�£���
    [Header("Resolve Timing")]
    [SerializeField] private float thumbSpinnerResolveSeconds = 1.2f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Vector3 popupWorldOffset = new Vector3(0f, 1.2f, 0f);
    private Coroutine attackWindowRoutine;
    private Coroutine resolveRoutine;
    private bool ifMovementLocked;
    [Header("Attack Window")]
    [SerializeField] private float hitWindowSeconds = 0.2f;
    [SerializeField] private ThumbSpinner thumbSpinnerPrefab;
    private const string ATTACK_TRIGGER = "Attack";
    private const string IDLE_TRIGGER = "Idle";

    // 新增：Awake方法自动获取/创建AudioSource
    private void Awake()
    {
        if (audioSource == null)
        {
            // 先获取现有AudioSource，没有则自动添加
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        }
        // 设置音效基础属性：不循环、不自动播放
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // ��������ʼ��ʱ����ԭʼX������ֵ������Ĭ����1���ͱ���1����2�ͱ���2��
        originalScaleX = Mathf.Abs(transform.localScale.x);
    }
    private void FixedUpdate()
    {
        // �����жϹ���״̬������ʱ�޷��ƶ�
        if (!isAttacking && !ifMovementLocked)
        {
            GetDirectionKeyInput(); // ��ȡ���������
            HandleGroundMovement(); // �����ƶ�
            ApplyMovement(); // Ӧ���ƶ�
            HandleObjectFlip();
        }
    }

    private void Update()
    {
        // ���F��������Update��ⰴ����������
        CheckAttackInput();
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
    /// <summary>
    /// ��ȡ�������ҷ�������루����Ӧ�����������ӦWASD��
    /// </summary>
    private void GetDirectionKeyInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        // �Ϸ����
        if (Input.GetKey(KeyCode.UpArrow))
        {
            verticalInput = 1f;
        }
        // �·����
        if (Input.GetKey(KeyCode.DownArrow))
        {
            verticalInput = -1f;
        }
        // �����
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput = -1f;
        }
        // �ҷ����
        if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput = 1f;
        }

        // ��һ����������������б���ƶ��ٶȹ���
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput).normalized;
        horizontalInput = inputVector.x;
        verticalInput = inputVector.y;
    }

    public void ApplySpeedChange(float delta)
    {
        moveSpeed += delta;
    }
    /// <summary>
    /// �������ӽ��ƶ��߼�����ԭ����һ�£�
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
            velocity.x = 0f;
            velocity.y = 0f;
        }
    }



    /// <summary>
    /// Ӧ���ƶ�����ɫλ��
    /// </summary>
    private void ApplyMovement()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    /// <summary>
    /// ���F����������
    /// </summary>
    private void CheckAttackInput()
    {
        // ����F�� + ���ڹ����� + ������ȴʱ��
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking && Time.time - lastAttackTime > attackCoolDown)
        {
            TriggerAttackAnimation();
        }
    }



    /// <summary>
    /// ������������
    /// </summary>
    private void TriggerAttackAnimation()
    {
        if (animator == null)
        {
            Debug.LogError("δ����Animator�����");
            return;
        }

        isAttacking = true;
        lastAttackTime = Time.time;
        velocity.x = 0f; // ����ʱֹͣ�ƶ�
        velocity.y = 0f;

        // 新增：播放攻击音效（有音效文件和AudioSource才播放）
        if (attackSoundClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSoundClip, attackSoundVolume);
        }

        animator.SetTrigger(ATTACK_TRIGGER); 
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
        animator.SetTrigger(IDLE_TRIGGER); 
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
        {
        
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        // 新增：自动赋值AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void HandleObjectFlip()
    {
        if (Mathf.Abs(horizontalInput) > inputDeadZone)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = originalScaleX * Mathf.Sign(horizontalInput);
            transform.localScale = newScale;
        }
    }


    public void LockMovement(bool locked)
    {
        ifMovementLocked = locked;
    }
}