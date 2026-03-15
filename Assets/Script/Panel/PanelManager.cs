using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 面板管理器：齿轮按钮暂停游戏 + 弹出设置预制体（原GameManager重命名）
/// </summary>
public class PanelManager : MonoBehaviour // 【修改1】类名从GameManager改为PanelManager
{
    [Header("核心接口（拖拽赋值）")]
    [Tooltip("齿轮形状的设置按钮（UI Button）")]
    public Button settingButton; // 齿轮按钮接口

    [Tooltip("设置面板预制体（需包含音量条+关闭按钮）【预制体拖拽接口】")]
    public GameObject settingPanelPrefab; // 【保留】预制体拖拽接口，可视化赋值

    private GameObject spawnedPanel; // 已生成的设置面板（防止重复创建）
    private bool isGamePaused = false; // 游戏暂停标记

    private void Start()
    {
        // 绑定齿轮按钮的点击事件
        if (settingButton != null)
        {
            settingButton.onClick.AddListener(ToggleSettingPanel);
        }
        else
        {
            Debug.LogWarning("未绑定齿轮设置按钮！", this);
        }
    }

    /// <summary>
    /// 核心：点击齿轮按钮 → 暂停游戏 + 弹出设置面板
    /// </summary>
    private void ToggleSettingPanel()
    {
        if (isGamePaused)
        {
            // 已暂停 → 关闭面板+恢复游戏
            CloseSettingPanel();
        }
        else
        {
            // 未暂停 → 暂停游戏+生成面板
            PauseGame();
            SpawnSettingPanel();
        }
    }

    /// <summary>
    /// 暂停游戏（Time.timeScale=0）
    /// </summary>
    private void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0; // 暂停所有基于Time.timeScale的逻辑
        Debug.Log("游戏已暂停");
    }

    /// <summary>
    /// 恢复游戏（Time.timeScale=1）【供SettingPanel调用】
    /// </summary>
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1; // 恢复游戏时间
        Debug.Log("游戏已恢复");
    }

    /// <summary>
    /// 生成设置面板（实例化预制体，核心预制体接口使用处）
    /// </summary>
    private void SpawnSettingPanel()
    {
        if (settingPanelPrefab == null)
        {
            Debug.LogWarning("未绑定设置面板预制体！请在Inspector拖拽预制体到settingPanelPrefab接口", this);
            return;
        }

        // 防止重复生成面板
        if (spawnedPanel != null) return;

        // 找到场景中的Canvas（面板必须在Canvas下显示）
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("场景中没有Canvas！请先创建Canvas", this);
            return;
        }

        // 【核心】实例化拖拽进来的预制体到Canvas下
        spawnedPanel = Instantiate(settingPanelPrefab, canvas.transform);
        // 给面板绑定PanelManager引用
        SettingPanel panelScript = spawnedPanel.GetComponent<SettingPanel>();
        if (panelScript != null)
        {
            panelScript.Init(this); // 【修改2】传递PanelManager引用（原GameManager）
        }
        else
        {
            Debug.LogWarning("设置面板预制体未挂载SettingPanel脚本！", this);
        }
    }

    /// <summary>
    /// 关闭设置面板
    /// </summary>
    public void CloseSettingPanel()
    {
        if (spawnedPanel != null)
        {
            Destroy(spawnedPanel);
            spawnedPanel = null;
        }
        ResumeGame(); // 关闭面板同时恢复游戏
    }
}