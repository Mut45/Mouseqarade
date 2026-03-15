using UnityEngine;

/// <summary>
/// 独立BGM管理器：通过TimeManager的cycleCount控制BGM切换
/// 无需耦合到TimeManager内部，仅需拖入TimeManager引用即可
/// </summary>
[RequireComponent(typeof(AudioSource))] // 自动添加AudioSource组件
public class BGMManager : MonoBehaviour
{
    [Header("核心引用")]
    [Tooltip("拖入场景中的TimeManager物体（获取cycleCount）")]
    public TimeManager timeManager;

    [Header("BGM配置")]
    [Tooltip("游戏初始播放的BGM")]
    public AudioClip initialBgm;
    [Tooltip("cycleCount达到指定值时切换的BGM（默认2=60秒）")]
    public AudioClip switchedBgm;
    [Tooltip("触发BGM切换的cycleCount阈值（默认2，即第2个30秒周期）")]
    public int switchCycleThreshold = 2;

    [Header("音频设置")]
    [Tooltip("BGM音量（0-1）")]
    [Range(0f, 1f)] public float bgmVolume = 0.8f;
    [Tooltip("是否循环播放BGM")]
    public bool isLoop = true;

    // 组件引用
    private AudioSource bgmAudioSource;
    // 标记：是否已切换过BGM（避免重复切换）
    private bool hasSwitchedBgm = false;
    // 记录上一帧的cycleCount，避免重复检测
    private int lastCycleCount = 0;

    #region 初始化
    private void Awake()
    {
        // 自动获取自身的AudioSource组件
        bgmAudioSource = GetComponent<AudioSource>();
        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // 初始化AudioSource设置
        InitAudioSource();
    }

    private void Start()
    {
        // 校验引用
        if (timeManager == null)
        {
            Debug.LogError("未拖入TimeManager引用！请在Inspector面板绑定", this);
            return;
        }

        // 播放初始BGM
        PlayInitialBgm();
    }

    /// <summary>
    /// 初始化AudioSource参数（不受Time.timeScale影响）
    /// </summary>
    private void InitAudioSource()
    {
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.loop = isLoop;
        // 旧版本Unity无需设置timeScaleMode，默认不受Time.timeScale影响
        bgmAudioSource.playOnAwake = false; // 禁止自动播放，由脚本控制
    }

    /// <summary>
    /// 播放初始BGM
    /// </summary>
    private void PlayInitialBgm()
    {
        if (initialBgm == null)
        {
            Debug.LogWarning("未配置初始BGM音频！", this);
            return;
        }

        bgmAudioSource.clip = initialBgm;
        bgmAudioSource.Play();
        Debug.Log("开始播放初始BGM");
    }
    #endregion

    #region 核心逻辑：检测cycleCount并切换BGM
    private void Update()
    {
        // 校验TimeManager引用
        if (timeManager == null) return;

        // 获取当前cycleCount
        int currentCycleCount = timeManager.GetCycleCount();

        // 仅当cycleCount变化且未切换过BGM时检测
        if (currentCycleCount != lastCycleCount && !hasSwitchedBgm)
        {
            CheckAndSwitchBgm(currentCycleCount);
            lastCycleCount = currentCycleCount; // 更新上一帧计数
        }
    }

    /// <summary>
    /// 检测cycleCount是否达到切换阈值，触发BGM切换
    /// </summary>
    /// <param name="currentCycleCount">当前30秒周期计数</param>
    private void CheckAndSwitchBgm(int currentCycleCount)
    {
        // 达到切换阈值（默认2=60秒）
        if (currentCycleCount >= switchCycleThreshold)
        {
            if (switchedBgm == null)
            {
                Debug.LogError("未配置切换后的BGM音频！", this);
                return;
            }

            // 切换BGM
            bgmAudioSource.clip = switchedBgm;
            bgmAudioSource.Play();
            hasSwitchedBgm = true; // 标记已切换，避免重复操作
            Debug.Log($"CycleCount达到{switchCycleThreshold}，已切换到第二阶段BGM");
        }
    }
    #endregion

    #region 外部调用接口（供其他脚本交互）
    /// <summary>
    /// 手动切换到初始BGM（比如游戏重置时）
    /// </summary>
    public void SwitchToInitialBgm()
    {
        if (initialBgm == null) return;

        bgmAudioSource.clip = initialBgm;
        bgmAudioSource.Play();
        hasSwitchedBgm = false; // 重置切换标记
        lastCycleCount = 0; // 重置计数记录
        Debug.Log("手动切换回初始BGM");
    }

    /// <summary>
    /// 手动切换到第二阶段BGM
    /// </summary>
    public void SwitchToSwitchedBgm()
    {
        if (switchedBgm == null) return;

        bgmAudioSource.clip = switchedBgm;
        bgmAudioSource.Play();
        hasSwitchedBgm = true;
        Debug.Log("手动切换到第二阶段BGM");
    }

    /// <summary>
    /// 暂停BGM
    /// </summary>
    public void PauseBgm()
    {
        bgmAudioSource.Pause();
    }

    /// <summary>
    /// 继续播放BGM
    /// </summary>
    public void ResumeBgm()
    {
        bgmAudioSource.UnPause();
    }

    /// <summary>
    /// 设置BGM音量
    /// </summary>
    /// <param name="volume">0-1的音量值</param>
    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmAudioSource.volume = bgmVolume;
    }
    #endregion
}