using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatPlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints; 
    [SerializeField] private GameObject ratPlayerPrefab;
    [SerializeField] private NPCRole[] availableRoles;

    private void Start()
    {
        int idx = Random.Range(0, spawnPoints.Length);
        var ratPlayer = Instantiate(ratPlayerPrefab, spawnPoints[idx].position, Quaternion.identity);
        int roleIdx = Random.Range(0, availableRoles.Length);
        ratPlayer.GetComponent<PlayerRoleApplier>().ApplyRole(availableRoles[roleIdx]);
    }
}
