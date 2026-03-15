using System;
using UnityEngine;

/// <summary>
/// Rat����F���������ٵĻ��ؽű�
/// �����ڻ��������ϣ�������Collider2D����ѡIs Trigger��
/// ���йؼ�����������Inspector����
/// </summary>
public class RatSpeedUpTrigger : MonoBehaviour
{
    [Header("�������ã����Զ��壩")]
    [Tooltip("�������ص�Rat��ǩ�������Rat�������ö�Ӧ��ǩ��")]
    public string ratTag = "Rat";
    [Tooltip("�����ָ����ٵ�CatAttack��ǩ���������Ӧ�������ñ�ǩ��")]
    public string catAttackTag = "CatAttack"; // ������CatAttack��ǩ����

    [Tooltip("����F���Ĵ���ʱ�����룩")]
    public float holdDuration = 2f; // Ĭ��2�룬����Inspector����

    [Tooltip("������Ҫ���õ�Time.timeScaleֵ��Ĭ��2���٣�")]
    public float targetTimeScale = 2f; // Ĭ��2������Inspector����

    [Header("������������")]
    [Tooltip("�Ƿ������һ�Σ�true=Rat�뿪�ٽ���Ҳ�޷��ظ�������")]
    public bool isOneTimeTrigger = false;

    [Tooltip("�������Ƿ������ٴδ�������Rat�뿪�ٽ��룩")]
    public bool allowReTrigger = true;

    [Header("���Բ����������޸ģ�")]
    [SerializeField] private bool isRatInTrigger; // Rat�Ƿ��ڻ��ط�Χ��
    [SerializeField] private float currentHoldTimer; // ��ǰ������ʱ
    [SerializeField] private bool isTriggered; // �Ƿ��Ѵ���������
    [SerializeField] private bool isTimeScaledUp; // ���������Ĳ���ֵ �� ��¼�Ƿ���ʱ�����״̬�����л�����Inspector���ԣ�
    [SerializeField] private bool isActive = true;
    [SerializeField] private Sprite normalStateSprite;
    [SerializeField] private Sprite brokenStateSprite;
    private SpriteRenderer sr;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    /// <summary>
    /// ��ʼ��״̬ + У������
    /// </summary>
    private void Start()
    {
        // ��ʼ��״̬����
        sr = GetComponent<SpriteRenderer>();
        isRatInTrigger = false;
        currentHoldTimer = 0f;
        isTriggered = false;
        isTimeScaledUp = false; // ��������ʼ��Ϊδ����״̬

        // У��Collider2D���ã�������Trigger��
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null || !collider.isTrigger)
        {
            Debug.LogError($"[{gameObject.name}] ����δ����Trigger��ײ�壡\n" +
                          "������BoxCollider2D/CircleCollider2D����ѡ��Is Trigger��", this);
        }

        // У�����������
        if (holdDuration <= 0)
        {
            holdDuration = 2f;
            Debug.LogWarning($"[{gameObject.name}] ����ʱ�����ܡ�0��������ΪĬ��ֵ2��", this);
        }
        if (targetTimeScale <= 0)
        {
            targetTimeScale = 2f;
            Debug.LogWarning($"[{gameObject.name}] Ŀ��TimeScale���ܡ�0��������ΪĬ��ֵ2", this);
        }
    }

    /// <summary>
    /// ֡���£����F������ + ��ʱ�߼� + ������ͬ������״̬����ֹ�ⲿ�޸�timeScale���²�ͬ����
    /// </summary>
    private void Update()
    {
        // ������ʵʱͬ������ֵ��ʵ��Time.timeScale�������ⲿ�ű��޸�timeScale���������ȱ©��
        SyncTimeScaleState();

        // ������һ�����Ѵ��� �� ֱ�ӷ��أ���ֹ�ظ�������
        if (isOneTimeTrigger && isTriggered) return;

        if (!isActive)
        {
            sr.sprite = brokenStateSprite;
            return;
        }
        else
        {
            sr.sprite = normalStateSprite;
        }

        // Rat���ڻ��ط�Χ�� �� ���ü�ʱ
        if (!isRatInTrigger)
        {
            currentHoldTimer = 0f;
            return;
        }

        // ���F������״̬
        if (Input.GetKey(KeyCode.E))
        {
            // ��Time.unscaledDeltaTime����ʱ���ܵ�ǰTimeScaleӰ�죨����Buff��ͣʱҲ��׼ȷ��ʱ��
            currentHoldTimer += Time.unscaledDeltaTime;

            // ���ԣ���ʾʣ�೤��ʱ��������1λС����
            float remainingTime = Mathf.Max(0, holdDuration - currentHoldTimer);
            Debug.Log($"����F���У�ʣ��ʱ����{remainingTime:F1}��", this);

            // �����ﵽָ��ʱ�� �� ��������
            if (currentHoldTimer >= holdDuration)
            {
                if (audioSource != null && audioClip != null)
                {
                    audioSource.PlayOneShot(audioClip);
                }
                TriggerTimeScaleChange();
            }
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            // �ɿ�F�� �� ���ü�ʱ
            currentHoldTimer = 0f;
            Debug.Log("�ɿ�F�������ó�����ʱ", this);
        }
    }

    /// <summary>
    /// ������ͬ��isTimeScaledUp��ʵ��Time.timeScale�����ķ�ȱ©�߼���
    /// ��ʹ�ⲿ�ű��޸���Time.timeScale��Ҳ�ܱ�֤����ֵ��׼ƥ��
    /// </summary>
    private void SyncTimeScaleState()
    {
        // �ж��߼���timeScale>1 �� ����״̬������ �� ����״̬
        bool actualScaledUp = Time.timeScale > 1f;
        if (isTimeScaledUp != actualScaledUp)
        {
            isTimeScaledUp = actualScaledUp;
            // ������־��״̬�仯ʱ��ʾ����ѡ�������Ų飩
            Debug.Log($"[{gameObject.name}] ʱ�����״̬���£�{isTimeScaledUp}����ǰTime.timeScale={Time.timeScale}��", this);
        }
    }

    /// <summary>
    /// ����TimeScale�޸ģ������߼����� ���������¼��ٲ���ֵ
    /// </summary>
    private void TriggerTimeScaleChange()
    {
        // ����Ŀ��TimeScale
        Time.timeScale = targetTimeScale;
        // ������ֱ�Ӹ��²���ֵ��ͬ��������
        isTimeScaledUp = targetTimeScale > 1f;

        // ����״̬���
        isTriggered = true;
        currentHoldTimer = 0f; // ���ü�ʱ

        // ������־
        Debug.Log($"[{gameObject.name}] �����ɹ���\n" +
              $"����F��{holdDuration}�� �� Time.timeScale = {targetTimeScale}������״̬��{isTimeScaledUp}", this);

        // �������ظ����� �� ֱ�ӷ���
        if (!allowReTrigger)
        {
            isOneTimeTrigger = true;
        }
    }

    #region Trigger��ײ��⣨Rat�������ط�Χ + CatAttack�����ָ����٣�
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ����Ƿ���Ŀ��Rat����
        if (other.CompareTag(ratTag))
        {
            isRatInTrigger = true;
            currentHoldTimer = 0f; // ����ʱ���ü�ʱ

            // ���ô�����ǣ������ٴδ�����
            if (allowReTrigger)
            {
                isTriggered = false;
            }

            Debug.Log($"Rat����[{gameObject.name}]���ط�Χ��\n" +
                  $"����F��{holdDuration}��ɽ���Ϸ�ٶ���Ϊ{targetTimeScale}��", this);
        }
        // ���������CatAttack��ǩ������� �� �ָ�1����
        else if (other.CompareTag("Player"))
        {
            ResetTimeScale();
            Debug.Log($"CatAttack�������[{gameObject.name}]���ط�Χ���ѻָ���Ϸ1���٣�����״̬��{isTimeScaledUp}", this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // ����Ƿ���Ŀ��Rat�뿪
        if (other.CompareTag(ratTag))
        {
            isRatInTrigger = false;
            currentHoldTimer = 0f; // �뿪ʱ���ü�ʱ
            Debug.Log($"Rat�뿪[{gameObject.name}]���ط�Χ��������ʱ������", this);
        }
    }
    #endregion

    #region �ⲿ���ƽӿڣ���ѡ�������ֶ�������Ч���� ��������ȡ����״̬�Ľӿ�
    /// <summary>
    /// �ֶ����û���״̬���������¿�ʼ��Ϸʱ���ã��� ���������ü��ٲ���ֵ
    /// </summary>
    public void ResetTrigger()
    {
        isRatInTrigger = false;
        currentHoldTimer = 0f;
        isTriggered = false;
        isTimeScaledUp = false; // ���������ü���״̬
        Debug.Log($"[{gameObject.name}] ����״̬�����ã�����״̬��{isTimeScaledUp}", this);
    }
    public void SetIsActive(bool active)
    {
        isActive = active;
    }
    /// <summary>
    /// �ֶ��ָ�Ĭ��TimeScale��1���٣��� ���������¼��ٲ���ֵ
    /// </summary>
    public void ResetTimeScale()
    {
        Time.timeScale = 1f;
        isTimeScaledUp = false; // �������ָ�Ϊδ����״̬
        Debug.Log($"�ֶ��ָ�Ĭ���ٶȣ�Time.timeScale = 1������״̬��{isTimeScaledUp}", this);
    }

    /// <summary>
    /// �����������ṩ��ȡ����״̬�Ľӿڣ�������Ч�ű��ɵ��ã�
    /// </summary>
    /// <returns>��ǰ�Ƿ���ʱ�����״̬</returns>
    public bool GetIsTimeScaledUp()
    {
        return isTimeScaledUp;
    }
    #endregion
    public bool IsTimeScaledUp() => isTimeScaledUp;
    public bool IsActive() => isActive;
}