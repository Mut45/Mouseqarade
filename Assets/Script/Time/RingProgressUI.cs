using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 总时长环形进度条UI组件（含颜色渐变）
/// 依赖TimeManager获取总计时数据，总时长多久就多久转一圈，支持初始/最终颜色平滑过渡
/// </summary>
public class RingProgressUI : MonoBehaviour
{
    [Header("依赖引用")]
    public TimeManager timeManager;
    public Image ringProgressImage;

    [Header("圆环刷新设置")]
    public float ringRefreshDelay = 2f;

    [Header("颜色渐变设置")] // 新增：颜色配置区域
    [Tooltip("圆环满圈时的初始颜色（比如绿色）")]
    public Color ringStartColor = Color.green;
    [Tooltip("圆环空圈时的最终颜色（比如红色）")]
    public Color ringEndColor = Color.red;
    [Tooltip("颜色渐变平滑速度（值越大过渡越快，默认5）")]
    public float colorSmoothSpeed = 5f;

    // 私有变量
    private float ringRefreshTimer;
    private bool isRingWaitingForRefresh;
    private float progressBeforeDelay;
    private bool isJustResetRing;
    private float resetMarkTimeoutTimer;
    public float resetMarkTimeout = 1f;
    private Color currentRingColor; // 新增：当前圆环颜色（用于平滑过渡）

    #region 初始化
    private void Start()
    {
        ringRefreshTimer = 0f;
        isRingWaitingForRefresh = false;
        progressBeforeDelay = 1f;
        isJustResetRing = false;
        resetMarkTimeoutTimer = 0f;
        currentRingColor = ringStartColor; // 初始化颜色为初始色

        UpdateRingProgress();
        UpdateRingColor(); // 初始化颜色

        if (timeManager == null)
        {
            Debug.LogError("未绑定TimeManager！请在Inspector面板拖拽TimeManager对象", this);
        }
        if (ringProgressImage == null)
        {
            Debug.LogError("未绑定环形进度条Image！请在Inspector面板拖拽Image组件", this);
        }
    }
    #endregion

    #region 圆环进度更新逻辑
    private void Update()
    {
        if (timeManager == null || ringProgressImage == null) return;
        if (!timeManager.IsCountingDown() || timeManager.remainingTime <= 0) return;

        // 1. 刷新延迟阶段：锁定进度和颜色，只计时
        if (isRingWaitingForRefresh)
        {
            ringProgressImage.fillAmount = progressBeforeDelay;
            ringProgressImage.color = currentRingColor; // 锁定当前颜色
            ringRefreshTimer += Time.deltaTime;

            if (ringRefreshTimer >= ringRefreshDelay)
            {
                // 延迟结束：重置状态 + 强制设为满圈 + 恢复初始色 + 标记刚重置
                isRingWaitingForRefresh = false;
                ringRefreshTimer = 0f;
                ringProgressImage.fillAmount = 1f;
                currentRingColor = ringStartColor;
                ringProgressImage.color = currentRingColor;
                isJustResetRing = true;
                resetMarkTimeoutTimer = 0f;
                Debug.Log($"圆环刷新延迟结束，强制重置为满圈（初始色）");
            }
            return;
        }

        // 2. 刚完成重置阶段：强制保持满圈和初始色，增加超时容错
        if (isJustResetRing)
        {
            ringProgressImage.fillAmount = 1f;
            currentRingColor = ringStartColor;
            ringProgressImage.color = currentRingColor; // 强制初始色
            resetMarkTimeoutTimer += Time.deltaTime;

            // 关闭标记的两个条件（满足其一即可，容错）：
            float remainingRatio = timeManager.remainingTime / timeManager.totalTime;
            bool cycleReset = remainingRatio > 0.99f;
            bool isTimeout = resetMarkTimeoutTimer >= resetMarkTimeout;

            if (cycleReset || isTimeout)
            {
                isJustResetRing = false;
                resetMarkTimeoutTimer = 0f;
                Debug.Log(cycleReset ? "总计时已重置，恢复正常进度更新" : "重置标记超时，强制恢复正常进度更新");
            }
            return;
        }

        // 3. 正常更新阶段：同步更新进度和颜色
        UpdateRingProgress();
        UpdateRingColor(); // 新增：更新颜色渐变

        // 检测是否转完一圈（总时长周期）
        float totalElapsed = timeManager.totalTime - timeManager.remainingTime;
        if (totalElapsed >= timeManager.totalTime - 0.1f)
        {
            progressBeforeDelay = ringProgressImage.fillAmount;
            isRingWaitingForRefresh = true;
            Debug.Log($"圆环转完一圈（总时长{timeManager.totalTime}秒），开始{ringRefreshDelay}秒刷新延迟");
        }
    }

    /// <summary>
    /// 计算并更新圆环进度（适配总时长周期）
    /// </summary>
    private void UpdateRingProgress()
    {
        float totalElapsed = timeManager.totalTime - timeManager.remainingTime;
        float ringRemainingTime = timeManager.totalTime - totalElapsed;
        ringRemainingTime = Mathf.Max(ringRemainingTime, 0);

        float progress = ringRemainingTime / timeManager.totalTime;
        ringProgressImage.fillAmount = progress;
    }

    /// <summary>
    /// 新增：平滑更新圆环颜色（从初始色过渡到最终色）
    /// </summary>
    private void UpdateRingColor()
    {
        if (ringProgressImage == null) return;

        // 目标颜色：根据当前进度插值（progress=1→初始色，progress=0→最终色）
        float progress = ringProgressImage.fillAmount;
        Color targetColor = Color.Lerp(ringEndColor, ringStartColor, progress);

        // 平滑过渡颜色（避免跳变）
        currentRingColor = Color.Lerp(currentRingColor, targetColor, Time.deltaTime * colorSmoothSpeed);
        ringProgressImage.color = currentRingColor;
    }
    #endregion

    #region 外部重置接口（可选）
    public void ResetRing()
    {
        isRingWaitingForRefresh = false;
        isJustResetRing = false;
        ringRefreshTimer = 0f;
        resetMarkTimeoutTimer = 0f;
        progressBeforeDelay = 1f;
        ringProgressImage.fillAmount = 1f;
        // 重置颜色为初始色
        currentRingColor = ringStartColor;
        ringProgressImage.color = currentRingColor;
        UpdateRingProgress();
    }
    #endregion
}