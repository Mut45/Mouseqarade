using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverSceneController : MonoBehaviour
{
    [Header("场景名称配置")]
    public string titleSceneName = "StartScene";
    public string gameSceneName = "InGameScene";

    [Header("按钮引用")]
    public Button backToTitleBtn;
    public Button restartBtn;

    private void Start()
    {
        // 打印初始化日志，确认脚本执行
        Debug.Log("GameOverSceneController 初始化中...");

        // 检查按钮赋值
        if (backToTitleBtn == null)
        {
            Debug.LogError("返回标题按钮未赋值！", this);
        }
        else
        {
            backToTitleBtn.onClick.AddListener(() =>
            {
                Debug.Log("点击了返回标题按钮，准备跳转：" + titleSceneName);
                BackToTitle();
            });
            Debug.Log("返回标题按钮绑定成功");
        }

        if (restartBtn == null)
        {
            Debug.LogError("重新开始按钮未赋值！", this);
        }
        else
        {
            restartBtn.onClick.AddListener(() =>
            {
                Debug.Log("点击了重新开始按钮，准备跳转：" + gameSceneName);
                RestartGame();
            });
            Debug.Log("重新开始按钮绑定成功");
        }
    }

    private void BackToTitle()
    {
        LoadTargetScene(titleSceneName);
    }

    private void RestartGame()
    {
        LoadTargetScene(gameSceneName);
    }

    private void LoadTargetScene(string sceneName)
    {
        Debug.Log("检查场景：" + sceneName + " 是否在Build Settings中");
        if (IsSceneInBuildSettings(sceneName))
        {
            Debug.Log("开始跳转场景：" + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"场景{sceneName}未添加到Build Settings！", this);
        }
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(path);
            if (sceneNameInBuild == sceneName)
            {
                Debug.Log("场景" + sceneName + "存在于Build Settings中");
                return true;
            }
        }
        return false;
    }
}