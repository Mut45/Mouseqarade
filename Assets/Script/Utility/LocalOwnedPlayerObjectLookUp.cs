using System.Net;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

public static class LocalOwnedPlayerLookUp
{
    public static bool TryGetLocalOwnedComponent<T>(out T component)
        where T : Component
    {
        component = null;
        T[] components = Object.FindObjectsByType<T>(FindObjectsSortMode.None);

        foreach (T candidate in components)
        {
            NetworkObject networkObject = candidate.GetComponentInParent<NetworkObject>();

            if (networkObject == null)
                continue;

            if (!networkObject.IsOwner)
                continue;

            component = candidate;
            return true;
        }

        return false;
    }

    public static T GetLocalOwnedComponent<T>() where T : Component
    {
        TryGetLocalOwnedComponent(out T component);
        return component;
    }

}