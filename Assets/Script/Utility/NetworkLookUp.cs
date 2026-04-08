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
}