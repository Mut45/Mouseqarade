using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MatchStartFlow : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string gameplaySceneName = "IngameScene";

    private bool ifPlayerSpawned = false;
    private bool ifSubscribed = false;
    private bool gameplaySceneLoaded = false;
    private bool gameplaySceneLoadRequested = false;
    public void InitAfterServerStart()
    {
        if (ifSubscribed) return;

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[MatchStartFlow] NetworkManager.Singleton is null.");
            return;
        }

        if (NetworkManager.Singleton.SceneManager == null)
        {
            Debug.LogError("[MatchStartFlow] NetworkSceneManager is still null.");
            return;
        }

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += HandleLoadCompleted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        ifSubscribed = true;
    }

    private void OnDisable()
    {
        if (ifSubscribed &&
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleLoadCompleted;
            ifSubscribed = false;
        }
    }
    
    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        int clientCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        Debug.Log($"[MatchStartFlow] Client connected: {clientId}. Total clients: {NetworkManager.Singleton.ConnectedClientsIds.Count}");

        if (!gameplaySceneLoaded && !gameplaySceneLoadRequested && clientCount == 2)
        {
            TryGameplaySceneLoad();
            return;
        }

        TrySpawnPlayers();
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        Debug.Log($"[MatchStartFlow] Client disconnected: {clientId}. Total clients: {NetworkManager.Singleton.ConnectedClientsIds.Count}");
    }

    private void HandleLoadCompleted(
        string sceneName, 
        UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, 
        System.Collections.Generic.List<ulong> clientsCompleted, 
        System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        if (ifPlayerSpawned) return;

        NetworkNPCSpawner npcSpawner = FindAnyObjectByType<NetworkNPCSpawner>();
        npcSpawner.InitNPCSpawning();
        
        gameplaySceneLoaded = true;
        TrySpawnPlayers(); 
    }

    private void TryGameplaySceneLoad()
    {
        if (gameplaySceneLoadRequested) return;

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        if (!NetworkManager.Singleton.NetworkConfig.EnableSceneManagement)
        {
            Debug.LogError("[MatchStartFlow] Network scene management is disabled.");
            return;
        }

        gameplaySceneLoadRequested = true;

        SceneEventProgressStatus loadStatus =
            NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);

        Debug.Log($"[MatchStartFlow] Gameplay scene load requested. Result: {loadStatus}");
    }

    private void TrySpawnPlayers()
    {
        if (ifPlayerSpawned) return;
        if (!gameplaySceneLoaded) return;

        int clientCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        if (clientCount != 2)
        {
            Debug.Log($"[MatchStartFlow] Waiting to spawn. GameplayLoaded={gameplaySceneLoaded}, ClientCount={clientCount}");
            return;
        }

        PlayerSpawnManager spawnManager = FindFirstObjectByType<PlayerSpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("[MatchStartFlow] No PlayerSpawnManager found in gameplay scene.");
            return;
        }

        ifPlayerSpawned = true;
        SpawnMatchPlayers(spawnManager);
    }

    private void SpawnMatchPlayers(PlayerSpawnManager spawnManager)
    {
        List<ulong> clientIds = new(NetworkManager.Singleton.ConnectedClientsIds);
        Debug.Log("[MatchStartFlow] The number of clients is: " + clientIds.Count);

        int catIndex = Random.Range(0, 2);
        ulong catClientId = clientIds[catIndex];
        ulong mouseClientId = clientIds[1 - catIndex];

        SpawnPlayerForClient(catClientId, PlayerRole.Cat, spawnManager);
        SpawnPlayerForClient(mouseClientId, PlayerRole.Mouse, spawnManager);
    }

    private void SpawnPlayerForClient(ulong clientId, PlayerRole role, PlayerSpawnManager spawnManager)
    {
        Transform spawnPoint = spawnManager.GetSpawnPointForRole(role);
        if (spawnPoint == null)
        {
            Debug.LogError($"[SessionGameFlow] Missing spawn point for role {role}.");
            return;
        }

        GameObject playerInstance = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        NetworkObject playerNO = playerInstance.GetComponent<NetworkObject>();
        if (playerNO == null)
        {
            Debug.LogError("[SessionGameFlow] Player prefab has no NetworkObject.");
            return;
        }

        playerNO.SpawnAsPlayerObject(clientId, true);

        PlayerRoleState roleState = playerInstance.GetComponent<PlayerRoleState>();
        if (roleState != null)
        {
            roleState.SetRole(role);
        }

    }
}