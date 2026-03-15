using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }
    private readonly List<NPCController> npcs = new();
    [SerializeField] private NPCRole[] roles;
    private Coroutine pauseAndThenResumeCoroutine;
    private bool isPaused;
    private bool isPendingRoleRandomize;

    // [Header("Testing")]
    // [SerializeField] private float testDuration = 5f;
    // [SerializeField] private float testPauseDuration = 3f;
    // private bool paused = false;
    // private float t;

    // void Update()
    // {
    //     t -= Time.deltaTime;
    //     if (t < 0) {
    //         PauseAndThenResume(testPauseDuration);
    //         // paused = true;
    //         t = testDuration;
    //     }
    // }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // t = testDuration;
    }

    public void Register(NPCController npc)
    {
        if (!npcs.Contains(npc))
            npcs.Add(npc);
    }

    public void Unregister(NPCController npc)
    {
        npcs.Remove(npc);
    }

    public void PauseAndThenResume(float pauseDuration)
    {
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
            npc.SetRole(roles[Random.Range(0, roles.Length)]);
        }
    }

    public IReadOnlyList<NPCController> GetAllNPCs() => npcs;
}
