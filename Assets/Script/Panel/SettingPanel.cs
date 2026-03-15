using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置面板逻辑：音量条调整 + 关闭按钮（适配PanelManager）
/// </summary>
public class SettingPanel : MonoBehaviour
{
    [Header("面板接口（拖拽赋值）")]
    [Tooltip("调整音量的滑动条（UI Slider）")]
    public Slider volumeSlider; // 音量条接口
    [Tooltip("关闭面板的按钮（UI Button）")]
    public Button closeButton; // 关闭按钮接口

    private PanelManager panelManager; // 【修改3】从GameManager改为PanelManager

    /// <summary>
    /// 初始化：接收PanelManager引用 + 绑定按钮/音量条事件
    /// </summary>
    /// <param name="pm">PanelManager实例</param>
    public void Init(PanelManager pm) // 【修改4】参数从GameManager改为PanelManager
    {
        panelManager = pm;

        // 绑定关闭按钮事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }
        else
        {
            Debug.LogWarning("设置面板未绑定关闭按钮！", this);
        }

        // 绑定音量条事件 + 初始化音量
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        else
        {
            Debug.LogWarning("设置面板未绑定音量滑动条！", this);
        }
    }

    /// <summary>
    /// 音量条拖动 → 调整全局音量
    /// </summary>
    private void OnVolumeChanged(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log($"音量已调整为：{volume:F1}");
    }

    /// <summary>
    /// 点击关闭按钮 → 通知PanelManager关闭面板+恢复游戏
    /// </summary>
    private void OnCloseButtonClick()
    {
        if (panelManager != null)
        {
            panelManager.CloseSettingPanel(); // 【修改5】调用PanelManager的方法
        }
    }
}