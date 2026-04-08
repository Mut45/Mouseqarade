
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class NetworkNPCManager : MonoBehaviour
{
    public static NetworkNPCManager Instance { get; private set; }
    [SerializeField] private List<NetworkNPCController> npcs = new();
    [SerializeField] private NPCRole[] roles;
    private Coroutine pauseAndThenResumeCoroutine;
    private bool isPaused;
    private bool isPendingRoleRandomize;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(NetworkNPCController npc)
    {
        if (!npcs.Contains(npc))
            npcs.Add(npc);
    }

    public void Unregister(NetworkNPCController npc)
    {
        npcs.Remove(npc);
    }

    public void PauseAndThenResume(float pauseDuration)
    {
        if (!NetworkManager.Singleton.IsServer) return; // using the NetworkManager since I want to make this class stay as MonoBehaviour

        if (pauseAndThenResumeCoroutine != null)
        {
            StopCoroutine(pauseAndThenResumeCoroutine);
        }
        pauseAndThenResumeCoroutine = StartCoroutine(PauseAllAndThenResumeCoroutine(pauseDuration));
    }

    private IEnumerator PauseAllAndThenResumeCoroutine(float pauseDuration)
    {
        isPaused = true;
        foreach (var npc in npcs)
        {
            if (npc == null) continue;
            npc.PauseMovement();
        }
        yield return new WaitForSeconds(pauseDuration);
        foreach (var npc in npcs)
        {
            if (npc == null) continue;
            npc.ResumeMovement();
        }
        isPaused = false;
        if (isPendingRoleRandomize)
        {
            isPendingRoleRandomize = false;
            RandomizeRoles();
        }
    }

    public void RandomizeRoles()
    {
        if (!NetworkManager.Singleton.IsServer) return; // using the NetworkManager since I want to make this class stay as MonoBehaviour

        if (roles == null || roles.Length == 0)
            return;

        if (isPaused)
        {
            isPendingRoleRandomize = true;
            return;
        }
        foreach (var npc in npcs)
        {
            if (npc == null) continue;
            npc.SetRoleById(Random.Range(0, roles.Length));
        }
    }

    public IReadOnlyList<NetworkNPCController> GetAllNetworkNPCs() => npcs;
}