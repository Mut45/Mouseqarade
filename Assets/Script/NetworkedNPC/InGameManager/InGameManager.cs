using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class InGameManager : NetworkBehaviour
{
    public static InGameManager Instance {get; private set;}

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void OnMouseBeingCaught()
    {
        if (!IsServer) return;
        Debug.Log("[InGameManager] Cat wins");
    }
}