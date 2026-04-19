using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform catPlayerSpawnPoint;
    [SerializeField] private Transform[] mousePlayerSpawnPoints;



    public Transform GetSpawnPointForRole(PlayerRole role)
    {
        if (mousePlayerSpawnPoints == null || catPlayerSpawnPoint == null)
        {
            return null;
        }
        int mouseRandomIndex = Random.Range(0, mousePlayerSpawnPoints.Length);
        switch (role)
        {
            case PlayerRole.Mouse:
                return mousePlayerSpawnPoints[mouseRandomIndex];
            case PlayerRole.Cat:
                return catPlayerSpawnPoint;
            default:
                return null;
        }
    }
}