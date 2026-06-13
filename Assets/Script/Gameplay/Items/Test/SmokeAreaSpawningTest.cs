using Unity.Netcode;
using UnityEngine;

public class SmokeNetworkSpawnTest : MonoBehaviour
{
    [SerializeField] private SmokeAOEAreaController smokePrefab;
    [SerializeField] private Vector2 spawnPosition = Vector2.zero;
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float duration = 10f;

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
        {

            Debug.Log("[SmokeAreaSpawningTest] Server started");
            NetworkManager.Singleton.StartHost();
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton == null) return;
        if (!NetworkManager.Singleton.IsServer) return;
        //Debug.Log($"Is server? :{NetworkManager.Singleton.IsServer}" );
        if (smokePrefab == null)
        {
            SmokeAOEAreaController[] smokes = FindObjectsByType<SmokeAOEAreaController>(FindObjectsSortMode.None);
            if (smokes.Length > 0)
            {
                smokePrefab = smokes[0];
            }
            
        }

        if (Input.GetKeyDown(KeyCode.T) && NetworkManager.Singleton != null)
        {
            SmokeAOEAreaController smoke = Instantiate(
                smokePrefab,
                spawnPosition,
                Quaternion.identity
            );

            NetworkObject networkObject = smoke.GetComponent<NetworkObject>();
            networkObject.Spawn(true);

            smoke.StartAOEEffectFromServer(spawnPosition, radius, duration);
        }
    }
}