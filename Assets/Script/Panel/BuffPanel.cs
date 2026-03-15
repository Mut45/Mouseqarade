using UnityEngine;
using System.Collections;

public class BuffSpawnManager : MonoBehaviour
{
    [Header("6个Buff预制体（顺序固定）")]
    public GameObject buffPrefab1;
    public GameObject buffPrefab2;
    public GameObject buffPrefab3;
    public GameObject buffPrefab4;
    public GameObject buffPrefab5;
    public GameObject buffPrefab6;

    [Header("Buff生成位置（Canvas下的RectTransform）")]
    public RectTransform spawnPos1;
    public RectTransform spawnPos2;

    [Header("动画配置")]
    public float scaleUpValue = 1.2f; // 放大的目标比例
    public float animationDuration = 0.3f; // 动画总时长（秒）
    // 自定义缓动曲线（可在Inspector拖拽调整）
    public AnimationCurve scaleUpCurve = AnimationCurve.EaseInOut(0, 0, 0.5f, 1);
    public AnimationCurve scaleDownCurve = AnimationCurve.EaseInOut(0, 1, 0.5f, 0);

    [Header("生成卡牌音效（新增）")]
    [SerializeField] private AudioSource spawnAudioSource; // 播放生成音效的AudioSource
    [SerializeField] private AudioClip spawnBuffClip; // 生成卡牌的音效文件
    [Range(0f, 1f)] public float spawnSoundVolume = 0.8f; // 音效音量

    public Canvas buffCanvas;

    private int currentRound = 0;
    private GameObject[][] fixedBuffGroups;
    public static BuffSpawnManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        fixedBuffGroups = new GameObject[3][]
        {
            new GameObject[] { buffPrefab1, buffPrefab2 },
            new GameObject[] { buffPrefab3, buffPrefab4 },
            new GameObject[] { buffPrefab5, buffPrefab6 }
        };

        // 校验Canvas配置
        if (buffCanvas == null)
        {
            buffCanvas = FindAnyObjectByType<Canvas>();
            if (buffCanvas == null)
            {
                Debug.LogError("场景中未找到Canvas！请先创建Canvas并赋值给buffCanvas");
            }
        }

        // 新增：初始化生成音效的AudioSource（容错处理）
        InitSpawnAudioSource();
    }

    // 新增：初始化音效组件，避免空引用
    private void InitSpawnAudioSource()
    {
        // 如果没手动赋值，自动获取当前物体上的AudioSource
        if (spawnAudioSource == null)
        {
            spawnAudioSource = GetComponent<AudioSource>();
            // 没有的话自动添加
            if (spawnAudioSource == null)
            {
                spawnAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        // 配置音效组件（不循环、不自动播放）
        spawnAudioSource.loop = false;
        spawnAudioSource.playOnAwake = false;
        spawnAudioSource.volume = spawnSoundVolume;
    }

    public void SpawnBuffForCurrentRound()
    {
        if (currentRound >= 3)
        {
            Debug.Log("所有Buff轮次已生成完毕");
            return;
        }

        // 前置校验
        if (buffCanvas == null)
        {
            Debug.LogError("Canvas未配置！无法生成UI Buff");
            return;
        }
        if (spawnPos1 == null || spawnPos2 == null)
        {
            Debug.LogError("UI生成位置未配置！请拖入Canvas下的RectTransform");
            return;
        }

        currentRound++;
        var currentGroup = fixedBuffGroups[currentRound - 1];
        GameObject prefab1 = currentGroup[0];
        GameObject prefab2 = currentGroup[1];

        if (prefab1 == null || prefab2 == null)
        {
            Debug.LogError("Buff预制体未配置！");
            return;
        }

        // ===================== 核心新增：生成卡牌前播放音效 =====================
        PlaySpawnBuffSound();

        // 生成Buff卡片实例
        GameObject buffInstance1 = Instantiate(prefab1, buffCanvas.transform);
        GameObject buffInstance2 = Instantiate(prefab2, buffCanvas.transform);

        // 获取RectTransform
        RectTransform rt1 = buffInstance1.GetComponent<RectTransform>();
        RectTransform rt2 = buffInstance2.GetComponent<RectTransform>();

        if (rt1 == null || rt2 == null)
        {
            Debug.LogError("Buff预制体缺少RectTransform组件！");
            return;
        }

        // 设置基础位置和尺寸
        rt1.anchoredPosition = spawnPos1.anchoredPosition;
        rt1.sizeDelta = spawnPos1.sizeDelta;
        rt1.rotation = spawnPos1.rotation;
        rt2.anchoredPosition = spawnPos2.anchoredPosition;
        rt2.sizeDelta = spawnPos2.sizeDelta;
        rt2.rotation = spawnPos2.rotation;

        // 播放缩放动画
        PlayScaleAnimation(rt1);
        PlayScaleAnimation(rt2);

        // 绑定BuffButtonHandler
        BuffButtonHandler handler1 = buffInstance1.GetComponent<BuffButtonHandler>();
        BuffButtonHandler handler2 = buffInstance2.GetComponent<BuffButtonHandler>();

        if (handler1 != null && handler2 != null)
        {
            handler1.otherBuffPrefab = buffInstance2;
            handler2.otherBuffPrefab = buffInstance1;
        }
        else
        {
            Debug.LogError("Buff预制体上未挂载BuffButtonHandler脚本！");
        }

        // 暂停游戏
        Time.timeScale = 0f;
        Debug.Log($"第{currentRound}轮Buff卡片已生成，游戏暂停");
    }

    // 新增：封装播放生成音效的方法（PlayOneShot）
    private void PlaySpawnBuffSound()
    {
        // 容错：AudioSource或音效文件为空时不报错
        if (spawnAudioSource == null || spawnBuffClip == null)
        {
            Debug.LogWarning("生成卡牌的AudioSource或音效文件未配置！");
            return;
        }
        // 播放一次性音效（不打断其他音效，不修改AudioSource默认Clip）
        spawnAudioSource.PlayOneShot(spawnBuffClip, spawnSoundVolume);
    }

    // 播放缩放动画（协程实现）
    private void PlayScaleAnimation(RectTransform targetRT)
    {
        StopCoroutine(ScaleAnimationCoroutine(targetRT)); // 停止旧协程避免冲突
        StartCoroutine(ScaleAnimationCoroutine(targetRT)); // 启动新协程
    }

    // 协程方法：实现先放大后恢复的动画
    private IEnumerator ScaleAnimationCoroutine(RectTransform targetRT)
    {
        targetRT.localScale = Vector3.zero; // 初始缩放为0
        float halfDuration = animationDuration / 2f;

        // 第一段：从0放大到scaleUpValue
        float elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // 不受Time.timeScale影响
            float t = elapsedTime / halfDuration;
            float scale = Mathf.Lerp(0, scaleUpValue, scaleUpCurve.Evaluate(t));
            targetRT.localScale = Vector3.one * scale;
            yield return null; // 等待下一帧
        }
        targetRT.localScale = Vector3.one * scaleUpValue;

        // 第二段：从scaleUpValue缩回到1
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

    public void ResetBuffRound()
    {
        currentRound = 0;
    }
}