using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffId
{
    CatMoveSpeed_Permanent = 0,
    CatMoveSpeed_OnFail_Temporary = 1,
    DisableClock = 2,

    RedLightGreenLight = 3,
    RatProximitySound = 4,
    RatInteractionSound = 5,
}

public class BuffManager : MonoBehaviour
{
    // 1. Permanently increase the movespeed of the cat player
    // 3. Disable the clock object in the game so the cat cant interact with it
    [SerializeField]private RatSpeedUpTrigger trigger;
    [SerializeField] private float redLightGreenLightSequenceDelayTime = 5.0f;
    [SerializeField] private CatPlayerMovement catPlayer;
    [SerializeField] private float tempSpeedBoostDuration = 5.0f;

    // Parameters for speed boost
    [SerializeField] private float tempSpeedBoostAmount = 2.0f;
    [SerializeField] private float permSpeedBoostAmount = 1.0f;
    private Coroutine tempSpeedBoostCoroutine;

    [Header("Buff 4: Red light green light")]
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private CountDownUI countDownUI;
    [SerializeField] private float redLightGreenLightDuration = 3.0f;

    //Proximity sound references
    [Header("Buff 5: Rat Proximity Sound Alert")]
    [SerializeField] private TopDownPlayerMovement ratPlayer;
    [SerializeField] private float proximityDistance = 3.0f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ratProximityClip;
    [SerializeField] private bool triggerOnEnterOnly = true;
    [SerializeField] private float proximityCooldown = 2f;
    [SerializeField] private float nextProximityTime;
    private bool wasInProximity;

    [Header("Buff 6: Rat Interact Sound Alert")]
    [SerializeField] private AudioClip interactSoundClip;

    // ===================== ������Buff��ȡ��Ч���ã������޸�1�� =====================
    [Header("Buff��ȡͨ����Ч")]
    [SerializeField] private AudioSource buffAcquireAudioSource; // ����Buff��ȡ��Ч��AudioSource
    [SerializeField] private AudioClip buffAcquireClip; // ����Buff���õĻ�ȡ��Ч��Inspector����ק��


    [SerializeField] private AudioSource tempMovespeedBuffAudioSource;
    [SerializeField] private AudioClip tempMovespeedBuffAudioClip;
    private bool tempSpeedActive = false;
    [SerializeField] private bool[] buffsIfOwned = new bool[6];

    void Update()
    {
        if (IsOwned(BuffId.RatProximitySound))
            TickRatProximitySound();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }


    }

    public static BuffManager Instance;

    [ContextMenu("TEST: Test Rat Interaction Sound")]
    public void TestAcquireBuff5()
    {
        AcquireBuff(BuffId.RatInteractionSound);
    }

    public void AcquireBuff(BuffId buff)
    {
        int idx = (int)buff;
        if (idx < 0 || idx >= buffsIfOwned.Length) return;

        // ��ǻ�ȡ��Buff
        buffsIfOwned[idx] = true;

        // ===================== ����������Buff��ȡ��Ч�������޸�3�� =====================
        PlayBuffAcquireSound();

        // ԭ��Buff�����߼�����ȫ������
        switch (buff)
        {
            case BuffId.CatMoveSpeed_Permanent:
                TriggerPermanentSpeedBoost();
                break;

            case BuffId.CatMoveSpeed_OnFail_Temporary:
                break;

            case BuffId.DisableClock:
                if (trigger != null)
                    trigger.SetIsActive(false);
                break;

            case BuffId.RedLightGreenLight:
                StartRedLightGreenLightSequence();
                break;

            case BuffId.RatProximitySound:
                wasInProximity = false;
                nextProximityTime = 0f;
                break;

            case BuffId.RatInteractionSound:
                RatPlayerOnInteract.OnRatInteracted += PlayRatInteractionSound;
                break;

            default:
                break;
        }
    }

    // ===================== ��������װ������Ч�ķ����������޸�4�� =====================
    private void PlayBuffAcquireSound()
    {
        // �ݴ���AudioSource��AudioClipΪ��ʱ������
        if (buffAcquireAudioSource == null || buffAcquireClip == null)
        {
            Debug.LogWarning("Buff��ȡ��Ч��AudioSource��AudioClipδ���ã�");
            return;
        }
        // ����һ����Ч��PlayOneShot�������ԭ�в��ţ�
        buffAcquireAudioSource.PlayOneShot(buffAcquireClip);
    }

    // ԭ�з�������ȫ���������޸ģ�
    [ContextMenu("TEST: Cat Permanent Speed Buff")]
    private void TriggerPermanentSpeedBoost()
    {
        catPlayer.ApplySpeedChange(permSpeedBoostAmount);
    }

    [ContextMenu("TEST: Cat Temp Speed Buff")]
    private void TriggerTemporarySpeedBoost()
    {

        if (!tempSpeedActive)
        {
            if (tempMovespeedBuffAudioSource != null && tempMovespeedBuffAudioClip != null)
            {
                Debug.Log("Sound Played !!!!");
                tempMovespeedBuffAudioSource.PlayOneShot(tempMovespeedBuffAudioClip);
            }
            tempSpeedActive = true;
            if (tempSpeedBoostCoroutine != null)
            {
                StopCoroutine(tempSpeedBoostCoroutine);
            }
            tempSpeedBoostCoroutine = StartCoroutine(TempSpeedBoostCoroutine());
        }
    }

    private IEnumerator TempSpeedBoostCoroutine()
    {
        Debug.Log("test started");
        catPlayer.ApplySpeedChange(tempSpeedBoostAmount);
        yield return new WaitForSeconds(tempSpeedBoostDuration);
        catPlayer.ApplySpeedChange(-tempSpeedBoostAmount);
    }

    public void NotifyFailedToCatchRat()
    {
        if (!IsOwned(BuffId.CatMoveSpeed_OnFail_Temporary)) return;
        TriggerTemporarySpeedBoost();
    }

    [ContextMenu("TEST: Red light green light")]
    public void StartRedLightGreenLightSequence()
    {
        StartCoroutine(RedLightGreenLightCoroutine());
    }

    private IEnumerator RedLightGreenLightCoroutine()
    {
        // TODO: start countdown
        if (countDownUI != null)
        {
            countDownUI.StartCountdown();
        }

        yield return new WaitForSeconds(redLightGreenLightSequenceDelayTime);
        npcManager.PauseAndThenResume(redLightGreenLightDuration);
    }

    [ContextMenu("TEST: Test Rat Proximity Sound")]
    private void TickRatProximitySound()
    {
        if (ratPlayer == null || catPlayer == null || audioSource == null || ratProximityClip == null)
            return;

        float dist = Vector2.Distance(ratPlayer.transform.position, catPlayer.transform.position);
        bool inRange = dist <= proximityDistance;

        if (triggerOnEnterOnly)
        {
            if (inRange && !wasInProximity)
                TryPlayProximitySound();
        }
        else
        {
            if (inRange)
                TryPlayProximitySound();
        }
        wasInProximity = inRange;
    }

    private void TryPlayProximitySound()
    {
        if (Time.time < nextProximityTime) return;
        audioSource.PlayOneShot(ratProximityClip);
        nextProximityTime = Time.time + Mathf.Max(0.05f, proximityCooldown);
    }

    private void PlayRatInteractionSound()
    {
        if (audioSource == null || interactSoundClip == null)
            return;
        Debug.Log("Rat Player Interacted!!");
        audioSource.PlayOneShot(interactSoundClip);
    }

    public bool IsOwned(BuffId buff)
    {
        int idx = (int)buff;
        if (idx < 0 || idx >= buffsIfOwned.Length) return false;
        return buffsIfOwned[idx];
    }

    private void OnDestroy()
    {
        RatPlayerOnInteract.OnRatInteracted -= PlayRatInteractionSound;
    }
}