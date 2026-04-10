using Unity.Netcode;

public static class NetworkLookUp
{
    public static bool TryGetNetworkNpc(NetworkManager networkManager, ulong networkObjectId, out NetworkNPCController npc)
    {
        npc = null;

        if (networkManager == null || networkManager.SpawnManager == null) return false;

        if (!networkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObject)) return false;
        
        npc = netObject.GetComponent<NetworkNPCController>();
        return npc != null;
    }

    public static bool TryGetPlayerObjectByRole(NetworkManager networkManager, PlayerRole targetRole, out NetworkObject playerObject)
    {
        playerObject = null;

        if (networkManager == null) return false;

        foreach (var kvp in networkManager.ConnectedClients)
        {
            NetworkObject candidate = kvp.Value.PlayerObject;
            if (candidate == null) continue;

            PlayerRoleState roleState = candidate.GetComponent<PlayerRoleState>();
            if (roleState == null) continue;

            if (roleState.GetRole() != targetRole) continue;

            playerObject = candidate;
            return true;
        }

        return false;
    }

    public static bool TryGetClientIdByRole(NetworkManager networkManager, PlayerRole targetRole, out ulong clientId)
    {
        clientId = default;

        if (networkManager == null) return false;

        foreach (var kvp in networkManager.ConnectedClients)
        {
            NetworkObject candidate = kvp.Value.PlayerObject;
            if (candidate == null) continue;

            PlayerRoleState roleState = candidate.GetComponent<PlayerRoleState>();
            if (roleState == null) continue;

            if (roleState.GetRole() != targetRole) continue;

            clientId = kvp.Key;
            return true;
        }

        return false;
    }
    
}