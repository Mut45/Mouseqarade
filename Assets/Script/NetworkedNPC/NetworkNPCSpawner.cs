using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkNPCSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkNPCController npcPrefab;
    [SerializeField] private Transform[] npcSpawnPoints;
    [SerializeField] private int npcSpawnCount = 20;
    [SerializeField] private float spawnRadius = 2.5f;
    [SerializeField] private LayerMask blockedLayers;
    [SerializeField] private int maxTriesPerNPC = 25;
    [SerializeField] private NPCRole[] roles;
    [SerializeField] private float spawnPointCircleOffset = 0.15f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        SpawnBatch();
    }
    public void SpawnBatch()
    {
        if (npcPrefab == null || roles == null || roles.Length == 0) return;
        for (int i = 0; i < npcSpawnCount; i++)
        {
            Transform center = npcSpawnPoints[i % npcSpawnPoints.Length];
            if (!TrySpawningAroundSP(center.position, out Vector3 pos))
                continue;
            NetworkNPCController npc = Instantiate(npcPrefab, pos, Quaternion.identity);
            npc.NetworkObject.Spawn(destroyWithScene: true);

            int assignedRoleId = Random.Range(0, roles.Length);
            npc.SetRoleById(assignedRoleId);

        }
    }

    private bool TrySpawningAroundSP(Vector2 center, out Vector3 pos)
    {
        for (int i = 0; i < maxTriesPerNPC; i++)
        {
            Vector2 candidate2D = center + Random.insideUnitCircle * spawnRadius;

            if (blockedLayers.value != 0)
            {
                if (Physics2D.OverlapCircle(candidate2D, spawnPointCircleOffset, blockedLayers) != null)
                    continue;
            }

            pos = new Vector3(candidate2D.x, candidate2D.y, 0f);
            return true;
        }

        pos = default;
        return false;
    }
}