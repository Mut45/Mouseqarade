using Unity.Netcode;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public enum GameWinner
{
    Cat,
    Mouse,
}
[RequireComponent(typeof(NetworkObject))]
public class InGameManager : NetworkBehaviour
{
    public static InGameManager Instance {get; private set;}
    [SerializeField] private NetworkVariable<float> clockBoostMultiplier = new(
        1f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Game Ending/Winning Scene References")]
    [SerializeField] private string mouseWinSceneName = "MouseWinScene";
    [SerializeField] private string catWinSceneName = "CatWinScene";

    public event Action<float> OnClockBoostMultiplierChanged;
    private bool hasGameEnded = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public override void OnNetworkSpawn()
    {
        clockBoostMultiplier.OnValueChanged += HandleClockBoostMultiplierChanged;
        OnClockBoostMultiplierChanged?.Invoke(clockBoostMultiplier.Value);
    }

    public override void OnNetworkDespawn()
    {
        clockBoostMultiplier.OnValueChanged -= HandleClockBoostMultiplierChanged;
    }

    #region Clock boost
    private void HandleClockBoostMultiplierChanged(float oldValue, float newValue)
    {
        OnClockBoostMultiplierChanged?.Invoke(newValue);
    }

    public float GetClockBoostMultiplier()
    {
        return clockBoostMultiplier.Value;
    }

    public bool IsClockBoostActive()
    {
        return clockBoostMultiplier.Value > 1f;
    }

    public void ActivateClockBoost()
    {
        if (!IsServer) return;
        clockBoostMultiplier.Value = 2.0f;
    }

    public void DeactivateClockBoost()
    {
        if (!IsServer) return;
        clockBoostMultiplier.Value = 1.0f;
    }
    #endregion
    
    #region Winning situation handlers
    public void OnMouseCaught()
    {
        if (!IsServer) return;
        
        EndGame(GameWinner.Cat);
        Debug.Log("[InGameManager] Cat wins");
    }

    public void OnTimeRanOut()
    {
        if (!IsServer) return;

        EndGame(GameWinner.Mouse);
    }
    private void EndGame(GameWinner winner)
    {
        if (!IsServer) return;
        if (hasGameEnded) return;

        NetworkTimeManager timeManager = UnityEngine.Object.FindFirstObjectByType<NetworkTimeManager>();
        if (timeManager != null)
        {
            timeManager.StopCountdown();
        }

        string sceneToLoad = winner == GameWinner.Cat ? catWinSceneName : mouseWinSceneName;
        NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }
    #endregion
}