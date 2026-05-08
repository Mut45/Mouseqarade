using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverSceneController : MonoBehaviour
{
    [Header("������������")]
    public string titleSceneName = "StartScene";
    public string gameSceneName = "InGameScene";

    [Header("��ť����")]
    public Button backToTitleBtn;
    public Button restartBtn;

    private void Start()
    {
        // ��ӡ��ʼ����־��ȷ�Ͻű�ִ��
        Debug.Log("GameOverSceneController ��ʼ����...");

        // ��鰴ť��ֵ
        if (backToTitleBtn == null)
        {
            Debug.LogError("���ر��ⰴťδ��ֵ��", this);
        }
        else
        {
            backToTitleBtn.onClick.AddListener(() =>
            {
                Debug.Log("����˷��ر��ⰴť��׼����ת��" + titleSceneName);
                BackToTitle();
            });
            Debug.Log("���ر��ⰴť�󶨳ɹ�");
        }

        if (restartBtn == null)
        {
            Debug.LogError("���¿�ʼ��ťδ��ֵ��", this);
        }
        else
        {
            restartBtn.onClick.AddListener(() =>
            {
                Debug.Log("��������¿�ʼ��ť��׼����ת��" + gameSceneName);
                RestartGame();
            });
            Debug.Log("���¿�ʼ��ť�󶨳ɹ�");
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
        Debug.Log("��鳡����" + sceneName + " �Ƿ���Build Settings��");
        if (IsSceneInBuildSettings(sceneName))
        {
            Debug.Log("��ʼ��ת������" + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"����{sceneName}δ���ӵ�Build Settings��", this);
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
                Debug.Log("����" + sceneName + "������Build Settings��");
                return true;
            }
        }
        return false;
    }
}