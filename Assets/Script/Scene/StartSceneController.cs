using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ���������������������������Start��ť��ת����Ϸ������
/// </summary>
public class StartSceneController : MonoBehaviour
{
    [Header("������������")]
    [Tooltip("Ҫ��ת����Ϸ�������ƣ������Build Settingsһ�£�")]
    public string inGameSceneName = "InGameScene";
    [Tooltip("����������Start��ť����ק��ֵ�����ȶ���")]
    public Button startButton;
    public Button exitButton;

    private void Start()
    {
        // ��Start��ť����¼�
        if (startButton != null)
        {
            startButton.onClick.AddListener(SwitchToInGameScene);
        }
        else
        {
            Debug.LogError("δ��Start��ť������Inspector��קStart��ť��startButton�ӿ�", this);
        }
    }

    /// <summary>
    /// ���ģ���ת����Ϸ����
    /// </summary>
    private void SwitchToInGameScene()
    {
        // ��鳡���Ƿ���Build Settings�У����ݵͰ汾Unity��
        if (!IsSceneInBuildSettings(inGameSceneName))
        {
            Debug.LogError($"��Ϸ����{inGameSceneName}δ���ӵ�Build Settings��", this);
            return;
        }

        // ��������5.3+�汾�ĳ�������
        SceneManager.LoadScene(inGameSceneName);
        Debug.Log($"������������ת����Ϸ������{inGameSceneName}");
    }

    private static bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return true;
        }
        return false;
    }

    public void ExitGame()
    {
        Debug.Log("Exit button pressed!");
        Application.Quit();
    }
}