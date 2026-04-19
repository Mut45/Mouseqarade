using Unity.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayMenuLocalTest : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    [Header("Scenes")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";
    public void StartHostLocal()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null)
        {
            SetStatus("No NetworkManager.Singleton found.");
            return;
        }

        bool started = nm.StartHost();
        SetStatus($"StartHost returned: {started}");

        if (!started)
            return;

        Debug.Log($"[RelayMenu] IsServer={nm.IsServer}, IsHost={nm.IsHost}, IsListening={nm.IsListening}");

        if (!nm.NetworkConfig.EnableSceneManagement)
        {
            SetStatus("Network scene management is disabled on NetworkManager.");
            return;
        }

        SceneEventProgressStatus loadStatus =
            nm.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);

        MatchStartFlow matchFlow = FindFirstObjectByType<MatchStartFlow>();
        if (matchFlow != null)
        {
            matchFlow.InitAfterServerStart();
        }
        SetStatus($"Scene load request result: {loadStatus}");
        Debug.Log($"[RelayMenu] LoadScene returned: {loadStatus}");
    }

    public void StartClientLocal()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null)
        {
            SetStatus("No NetworkManager.Singleton found.");
            return;
        }

        bool started = nm.StartClient();
        SetStatus($"StartClient returned: {started}");
    }

    private void SetStatus(string message)
    {
        Debug.Log($"[RelayMenu] {message}");

        if (statusText != null)
            statusText.text = message;
    }
}