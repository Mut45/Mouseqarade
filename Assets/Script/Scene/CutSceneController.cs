using UnityEngine;
using UnityEngine.SceneManagement; // 场景切换核心命名空间

/// <summary>
/// 接收Timeline信号，切换到指定单个场景
/// 仅保留场景切换核心功能，无多余逻辑
/// </summary>
public class TimelineSceneSwitcher : MonoBehaviour
{
    [Header("场景切换配置")]
    [Tooltip("要切换到的目标场景名称（必须和Build Settings中一致）")]
    public string targetSceneName = "YourTargetScene"; // 手动指定目标场景名

    /// <summary>
    /// Timeline信号触发的核心方法（Timeline中绑定此方法）
    /// </summary>
    public void OnTimelineTriggerSceneSwitch()
    {
        // 校验场景名是否填写
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("未指定要切换的目标场景名！请在Inspector面板填写targetSceneName");
            return;
        }

        // 切换到指定场景（单场景加载模式，项目通用）
        SceneManager.LoadScene(targetSceneName);
        Debug.Log($"Timeline触发场景切换，前往：{targetSceneName}");
    }
}