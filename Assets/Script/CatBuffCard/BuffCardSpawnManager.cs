using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffCardSpawnManager : MonoBehaviour
{
    public static BuffCardSpawnManager Instance { get; private set; }

    [Header("Buff card prefabs in fixed order")]
    [SerializeField] private List<GameObject> buffCardPrefabs = new();

    [Header("Spawn positions")]
    [SerializeField] private RectTransform spawnPos1;
    [SerializeField] private RectTransform spawnPos2;

    [Header("Canvas")]
    [SerializeField] private Canvas buffCanvas;

    [Header("Animation")]
    [SerializeField] private float scaleUpValue = 1.2f;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleUpCurve = AnimationCurve.EaseInOut(0, 0, 0.5f, 1);
    [SerializeField] private AnimationCurve scaleDownCurve = AnimationCurve.EaseInOut(0, 1, 0.5f, 0);

    [Header("Audio")]
    [SerializeField] private AudioSource spawnAudioSource;
    [SerializeField] private AudioClip spawnBuffClip;
    [SerializeField, Range(0f, 1f)] private float spawnSoundVolume = 0.8f;

    private int currentRound = 0;
    private GameObject currentBuffInstance1;
    private GameObject currentBuffInstance2;
    private CatBuffChoiceController activeChoiceController;
    public CatBuffChoiceController ActiveChoiceController => activeChoiceController;
    private int TotalRounds => buffCardPrefabs.Count / 2;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (buffCanvas == null)
        {
            buffCanvas = FindAnyObjectByType<Canvas>();
            if (buffCanvas == null)
            {
                Debug.LogError("[BuffCardSpawnManager] No Canvas found in scene.", this);
            }
        }

        InitSpawnAudioSource();
        ValidateSetup();
    }

    private void InitSpawnAudioSource()
    {
        if (spawnAudioSource == null)
        {
            spawnAudioSource = GetComponent<AudioSource>();
            if (spawnAudioSource == null)
            {
                spawnAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        spawnAudioSource.loop = false;
        spawnAudioSource.playOnAwake = false;
        spawnAudioSource.volume = spawnSoundVolume;
    }

    private void ValidateSetup()
    {
        if (buffCardPrefabs.Count == 0)
        {
            Debug.LogError("[BuffCardSpawnManager] No buff card prefabs assigned.", this);
        }

        if (buffCardPrefabs.Count % 2 != 0)
        {
            Debug.LogError("[BuffCardSpawnManager] Buff card prefab count must be even.", this);
        }

        if (spawnPos1 == null || spawnPos2 == null)
        {
            Debug.LogError("[BuffCardSpawnManager] Spawn positions are not assigned.", this);
        }
    }

    public void SetActiveChoiceController(CatBuffChoiceController controller)
    {
        activeChoiceController = controller;
    }

    public void ClearActiveChoiceController(CatBuffChoiceController controller)
    {
        if (activeChoiceController == controller)
        {
            activeChoiceController = null;
        }
    }

    public void SpawnBuffForCurrentRound()
    {
        if (currentRound >= TotalRounds)
        {
            Debug.Log("[BuffCardSpawnManager] All buff rounds already spawned.");
            return;
        }

        if (buffCanvas == null || spawnPos1 == null || spawnPos2 == null)
        {
            Debug.LogError("[BuffCardSpawnManager] Missing canvas or spawn positions.", this);
            return;
        }

        int firstIndex = currentRound * 2;
        int secondIndex = firstIndex + 1;

        if (secondIndex >= buffCardPrefabs.Count)
        {
            Debug.LogError("[BuffCardSpawnManager] Buff card index out of range.", this);
            return;
        }

        GameObject prefab1 = buffCardPrefabs[firstIndex];
        GameObject prefab2 = buffCardPrefabs[secondIndex];

        if (prefab1 == null || prefab2 == null)
        {
            Debug.LogError("[BuffCardSpawnManager] Missing buff card prefab in list.", this);
            return;
        }

        CloseCurrentPanel();
        PlaySpawnBuffSound();

        currentBuffInstance1 = Instantiate(prefab1, buffCanvas.transform);
        currentBuffInstance2 = Instantiate(prefab2, buffCanvas.transform);

        SetupBuffCard(currentBuffInstance1, spawnPos1);
        SetupBuffCard(currentBuffInstance2, spawnPos2);

        currentRound++;
        Debug.Log($"[BuffCardSpawnManager] Spawned buff card panel for round {currentRound}.");
    }

    private void SetupBuffCard(GameObject buffInstance, RectTransform targetPos)
    {
        if (buffInstance == null || targetPos == null) return;

        RectTransform rt = buffInstance.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogError("[BuffCardSpawnManager] Buff prefab is missing RectTransform.", buffInstance);
            return;
        }

        rt.anchoredPosition = targetPos.anchoredPosition;
        rt.sizeDelta = targetPos.sizeDelta;
        rt.rotation = targetPos.rotation;

        PlayScaleAnimation(rt);
    }

    public void CloseCurrentPanel()
    {
        if (currentBuffInstance1 != null)
        {
            Destroy(currentBuffInstance1);
            currentBuffInstance1 = null;
        }

        if (currentBuffInstance2 != null)
        {
            Destroy(currentBuffInstance2);
            currentBuffInstance2 = null;
        }
    }

    public void ResetBuffRound()
    {
        currentRound = 0;
        CloseCurrentPanel();
        activeChoiceController = null;
    }

    private void PlaySpawnBuffSound()
    {
        if (spawnAudioSource == null || spawnBuffClip == null) return;
        spawnAudioSource.PlayOneShot(spawnBuffClip, spawnSoundVolume);
    }

    private void PlayScaleAnimation(RectTransform targetRT)
    {
        if (targetRT == null) return;
        StartCoroutine(ScaleAnimationCoroutine(targetRT));
    }

    private IEnumerator ScaleAnimationCoroutine(RectTransform targetRT)
    {
        targetRT.localScale = Vector3.zero;
        float halfDuration = animationDuration * 0.5f;

        float elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / halfDuration;
            float scale = Mathf.Lerp(0f, scaleUpValue, scaleUpCurve.Evaluate(t));
            targetRT.localScale = Vector3.one * scale;
            yield return null;
        }

        targetRT.localScale = Vector3.one * scaleUpValue;

        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / halfDuration;
            float scale = Mathf.Lerp(scaleUpValue, 1f, scaleDownCurve.Evaluate(t));
            targetRT.localScale = Vector3.one * scale;
            yield return null;
        }

        targetRT.localScale = Vector3.one;
    }
}