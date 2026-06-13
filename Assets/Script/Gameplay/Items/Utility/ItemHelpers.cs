using UnityEngine;
using Unity.Netcode;
public static class ItemHelpers
{
    public static Transform GetLocalPlayerTransform()
    {
        if (NetworkManager.Singleton == null) return null;
        if (NetworkManager.Singleton.LocalClient == null) return null;
        if (NetworkManager.Singleton.LocalClient.PlayerObject == null) return null;

        return NetworkManager.Singleton.LocalClient.PlayerObject.transform;
    }
}