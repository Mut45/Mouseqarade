using System.Collections;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private NPCController npcPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int spawnCount = 20;

    [Tooltip("NPC spawns within this radius around the chosen spawn point.")]
    [SerializeField] private float spawnRadius = 2.5f;

    [Tooltip("Optional: layers that block spawning (walls, obstacles).")]
    [SerializeField] private LayerMask blockedLayers;

    [Tooltip("How many times to retry finding a valid position per NPC.")]
    [SerializeField] private int maxTriesPerNPC = 25;

    [Header("Roles")]
    [SerializeField] private NPCRole[] roles;
    [SerializeField] private float randomSpawnCircleRadius = 0.15f;

    private void Start()
    {
        SpawnBatch();
    }
    public void SpawnBatch(){
         if (npcPrefab == null || roles == null || roles.Length == 0)
         {
            return;
        }
        for (int i = 0; i < spawnCount; i++)
        {
            Transform center = spawnPoints[i % spawnPoints.Length];
            if (!TrySpawningAroundSP(center.position, out Vector3 pos))
                continue;
            NPCController npc = Instantiate(npcPrefab, pos, Quaternion.identity);
            
            NPCRole assigned = roles[Random.Range(0, roles.Length)];
            npc.SetRole(assigned);

        }

    }
    private bool TrySpawningAroundSP(Vector2 center, out Vector3 pos)
    {
                for (int i = 0; i < maxTriesPerNPC; i++)
        {
            Vector2 candidate2D = center + Random.insideUnitCircle * spawnRadius;

            if (blockedLayers.value != 0)
            {
                if (Physics2D.OverlapCircle(candidate2D, randomSpawnCircleRadius, blockedLayers) != null)
                    continue;
            }

            pos = new Vector3(candidate2D.x, candidate2D.y, 0f);
            return true;
        }

        pos = default;
        return false;
    }

}
