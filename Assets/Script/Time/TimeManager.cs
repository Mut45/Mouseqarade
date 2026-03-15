using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 纯时间管理器：总倒计时 + 30秒触发NPC功能 + Legacy Text显示剩余时间 + cycleCount/stage状态
/// 无任何BGM相关逻辑，专注计时核心功能
/// </summary>
public class TimeManager : MonoBehaviour
{
    [Header("核心计时设置")]
    [Tooltip("总倒计时时长（秒），可在Inspector面板设置")]
    public float totalTime = 300f; // 默认5分钟
    [Tooltip("是否进入场景自动开始倒计时")]
    public bool autoStartCountdown = true;

    [Header("30秒间隔触发设置")]
    [Tooltip("绑定NPCManager物体上的指定方法（直接拖拽赋值）")]
    public UnityEvent onEvery30Seconds;

    [Header("剩余时间显示（Legacy Text）")]
    [Tooltip("拖入显示剩余时间的Legacy Text组件")]
    public Text remainingTimeText;
    [Tooltip("时间显示格式：是否显示为 分:秒（比如120秒→2:00）")]
    public bool showMinuteSecondFormat = true;

    // 对外暴露的30秒周期常量（供圆环UI/其他脚本读取）
    public readonly float ringTotalCycleTime = 30f;

    // 私有计时变量
    public float remainingTime;
    private float lastTriggerTime;
    private bool isCountingDown;

    // 30秒周期计数 & stage状态（核心）
    private int cycleCount = 0; // 初始为0的整数
    private bool stage = false; // 初始为false的bool

    #region 初始化
    private void Start()
    {
        // 初始化剩余时间和文本显示
        remainingTime = totalTime;
        UpdateRemainingTimeText();

        // 自动开始倒计时
        if (autoStartCountdown)
        {
            StartCountdown();
        }
    }
    #endregion

    #region 核心倒计时逻辑
    private void Update()
    {
        if (!isCountingDown) return;

        // 扣减剩余时间（防止负数）
        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(remainingTime, 0);

        // 更新时间显示文本
        UpdateRemainingTimeText();

        // 检测倒计时结束
        if (remainingTime <= 0)
        {
            StopCountdown();
            Debug.Log("总倒计时结束！");
            return;
        }

        // 30秒间隔触发逻辑
        float timeSinceLastTrigger = totalTime - remainingTime - lastTriggerTime;
        if (timeSinceLastTrigger >= ringTotalCycleTime)
        {
            TriggerEvery30Seconds();
            lastTriggerTime += ringTotalCycleTime;
        }
    }

    /// <summary>
    /// 更新剩余时间到Legacy Text组件
    /// </summary>
    private void UpdateRemainingTimeText()
    {
        if (remainingTimeText == null)
        {
            Debug.LogWarning("未绑定剩余时间显示的Legacy Text组件！", this);
            return;
        }

        string timeText = "";
        if (showMinuteSecondFormat)
        {
            // 分:秒格式（补零，比如5秒→0:05）
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timeText = $"{minutes}:{seconds:D2}";
        }
        else
        {
            // 带一位小数的秒数格式
            timeText = $"{remainingTime:F1}秒";
        }

        // 倒计时结束时的文本
        if (remainingTime <= 0)
        {
            timeText = "时间结束！";
        }

        remainingTimeText.text = $"剩余时间：{timeText}";
    }

    /// <summary>
    /// 30秒周期触发逻辑（更新计数+stage+调用绑定事件）
    /// </summary>
    private void TriggerEvery30Seconds()
    {
        // 1. 30秒周期计数+1
        cycleCount++;
        // 2. 更新stage状态（<=1→false，>1→true）
        stage = cycleCount > 1;

        // 调试日志（可选，方便查看状态，不需要可删除）
        Debug.Log($"30秒周期触发！当前计数：{cycleCount}，stage状态：{stage}");

        // 3. 触发绑定的NPCManager方法
        if (onEvery30Seconds != null && onEvery30Seconds.GetPersistentEventCount() > 0)
        {
            onEvery30Seconds.Invoke();
            Debug.Log($"倒计时剩余{remainingTime:F1}秒，触发30秒间隔的NPCManager功能！");
        }
        else
        {
            Debug.LogWarning("未绑定NPCManager的方法到onEvery30Seconds事件！");
        }
    }
    #endregion

    #region 外部调用接口（供其他脚本/UI/Timeline调用）
    /// <summary>
    /// 开始倒计时
    /// </summary>
    public void StartCountdown()
    {
        isCountingDown = true;
        lastTriggerTime = 0;
        Debug.Log($"开始总倒计时：{totalTime}秒");
    }

    /// <summary>
    /// 停止倒计时
    /// </summary>
    public void StopCountdown()
    {
        isCountingDown = false;
        UpdateRemainingTimeText();
    }

    /// <summary>
    /// 重置倒计时（可设置新时长）
    /// </summary>
    /// <param name="newTotalTime">新的总倒计时时长</param>
    public void ResetCountdown(float newTotalTime)
    {
        totalTime = newTotalTime;
        remainingTime = newTotalTime;
        lastTriggerTime = 0;
        isCountingDown = true;
        UpdateRemainingTimeText();
        Debug.Log($"重置倒计时，新总时长：{newTotalTime}秒");
    }

    /// <summary>
    /// 判断是否正在倒计时（供圆环UI/其他脚本读取）
    /// </summary>
    /// <returns>是否处于倒计时状态</returns>
    public bool IsCountingDown()
    {
        return isCountingDown;
    }

    /// <summary>
    /// 获取30秒周期内的已流逝时间（供圆环UI计算进度）
    /// </summary>
    /// <returns>当前周期内已流逝的秒数</returns>
    public float GetElapsedTimeIn30sCycle()
    {
        float totalElapsed = totalTime - remainingTime;
        return totalElapsed % ringTotalCycleTime;
    }

    /// <summary>
    /// 获取当前30秒周期计数（供BGM管理器/Timeline读取）
    /// </summary>
    /// <returns>已触发的30秒周期次数</returns>
    public int GetCycleCount()
    {
        return cycleCount;
    }

    /// <summary>
    /// 获取当前stage状态（供其他脚本判断逻辑）
    /// </summary>
    /// <returns>stage布尔值（<=1→false，>1→true）</returns>
    public bool GetStageState()
    {
        return stage;
    }

    /// <summary>
    /// 重置周期计数和stage状态（游戏重新开始时调用）
    /// </summary>
    public void ResetCycleCount()
    {
        cycleCount = 0;
        stage = false;
        Debug.Log("已重置30秒周期计数和stage状态");
    }
    #endregion
}