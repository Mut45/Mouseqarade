using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameSceneController : MonoBehaviour
{
    [Header("��Ϸ��������")]
    [Tooltip("èʤ���������ƣ�Build Settingsһ�£�")]
    public string catWinSceneName = "CatWin";
    [Tooltip("����ʤ���������ƣ�Build Settingsһ�£�")]
    public string ratWinSceneName = "RatWin";

    [Header("��Ϸ�����������")]
    [Tooltip("����RatPlayerController���������壨��ק��ֵ��")]
    public GameObject ratPlayer;
    [Tooltip("����TimeManager�ĵ���ʱ���壨��ק��ֵ��")]
    public GameObject timeManagerObj;

    // �������
    private TopDownPlayerMovement ratController;
    private TimeManager timeManager;
    private bool hasSwitchedScene = false; // ��ֹ�ظ���ת

    private void Start()
    {
        // ��ʼ�����ã�ֻ����Ϸ����ִ�У�
        InitReferences();
    }

    private void Update()
    {
        // ����ת������ֱ�ӷ��أ������ظ����
        if (hasSwitchedScene) return;

        // ���1������deadΪtrue �� ��èʤ������
        if (ratController != null && ratController.dead)
        {
            SwitchToCatWinScene();
        }

        // ���2������ʱ���� �� ������ʤ������
        if (timeManager != null && timeManager.remainingTime <= 0)
        {
            SwitchToRatWinScene();
        }
    }

    /// <summary>
    /// ��ʼ�����������������ʱ������������
    /// </summary>
    private void InitReferences()
    {
        // ��ʼ�����������
        if (ratPlayer != null)
        {
            ratController = ratPlayer.GetComponent<TopDownPlayerMovement>();
            if (ratController == null)
            {
                Debug.LogError($"��������{ratPlayer.name}δ����RatPlayerController�ű���", this);
            }
        }
        else
        {
            Debug.LogError("δ��ֵ�������嵽ratPlayer�ӿڣ�", this);
        }

        // ��ʼ������ʱ������
        if (timeManagerObj != null)
        {
            timeManager = timeManagerObj.GetComponent<TimeManager>();
            if (timeManager == null)
            {
                Debug.LogError($"����ʱ����{timeManagerObj.name}δ����TimeManager�ű���", this);
            }
        }
        else
        {
            Debug.LogError("δ��ֵ����ʱ���嵽timeManagerObj�ӿڣ�", this);
        }
    }

    /// <summary>
    /// ��ת��èʤ������
    /// </summary>
    private void SwitchToCatWinScene()
    {
        if (IsSceneInBuildSettings(catWinSceneName))
        {
            hasSwitchedScene = true;
            SceneManager.LoadScene(catWinSceneName);
            Debug.Log("�������� �� ��ת��èʤ��������" + catWinSceneName);
        }
        else
        {
            Debug.LogError($"èʤ������{catWinSceneName}δ���ӵ�Build Settings��", this);
        }
    }

    /// <summary>
    /// ��ת������ʤ������
    /// </summary>
    private void SwitchToRatWinScene()
    {
        if (IsSceneInBuildSettings(ratWinSceneName))
        {
            hasSwitchedScene = true;
            SceneManager.LoadScene(ratWinSceneName);
            Debug.Log("����ʱ���� �� ��ת������ʤ��������" + ratWinSceneName);
        }
        else
        {
            Debug.LogError($"����ʤ������{ratWinSceneName}δ���ӵ�Build Settings��", this);
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
                return true;
            }
        }
        return false;
    }
}