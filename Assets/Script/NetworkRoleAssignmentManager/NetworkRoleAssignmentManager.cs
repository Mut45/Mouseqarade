using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkRoleAssignmentManager : NetworkBehaviour
{
    public static NetworkRoleAssignmentManager Instance { get; private set; }

    private readonly List<PlayerRoleState> registeredPlayers = new();
    private bool rolesAssigned;

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
        if (!IsServer) return;

        NetworkManager.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.OnClientDisconnectCallback += HandleClientDisconnected;

        RegisterAlreadyConnectedPlayers();
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager == null) return;

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void RegisterAlreadyConnectedPlayers()
    {
        registeredPlayers.Clear();
        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            TryRegisterClientPlayer(client.ClientId);
        }

        TryAssignRoles();
    }

    private void HandleClientConnected(ulong clientId)
    {
        TryRegisterClientPlayer(clientId);
        TryAssignRoles();
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = registeredPlayers.Count - 1; i >= 0; i--)
        {
            PlayerRoleState roleState = registeredPlayers[i];

            if (roleState == null || roleState.OwnerClientId == clientId)
            {
                registeredPlayers.RemoveAt(i);
            }
        }

        rolesAssigned = false;
    }

    private void TryRegisterClientPlayer(ulong clientId)
    {
        if (!NetworkManager.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
            return;

        if (client.PlayerObject == null)
        {
            return;
        }

        PlayerRoleState roleState = client.PlayerObject.GetComponent<PlayerRoleState>();
        if (roleState == null)
        {
            return;
        }

        if (registeredPlayers.Contains(roleState))
            return;

        registeredPlayers.Add(roleState);
    }

    private void TryAssignRoles()
    {
        if (!IsServer) return;
        if (rolesAssigned) return;
        if (registeredPlayers.Count != 2) return;

        int catIndex = Random.Range(0, 2);
        int mouseIndex = 1 - catIndex;

        registeredPlayers[catIndex].SetRole(PlayerRole.Cat);
        registeredPlayers[mouseIndex].SetRole(PlayerRole.Mouse);

        rolesAssigned = true;

        Debug.Log(
            $"[NetworkRoleAssignmentManager] Roles assigned. " +
            $"Cat = client {registeredPlayers[catIndex].OwnerClientId}, " +
            $"Mouse = client {registeredPlayers[mouseIndex].OwnerClientId}"
        );
    }

    [ContextMenu("Debug Registered Players")]
    private void DebugRegisteredPlayers()
    {
        Debug.Log($"[NetworkRoleAssignmentManager] Registered count = {registeredPlayers.Count}");

        foreach (var player in registeredPlayers)
        {
            if (player == null)
            {
                continue;
            }

        }
    }
}