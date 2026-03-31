using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class RatPlayerOnInteract : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private PlayerRoleApplier roleApplier;

    private NPCController npcInRange;
    public static event Action OnRatInteracted;
    private void Awake()
    {
        if (roleApplier == null)
            roleApplier = GetComponent<PlayerRoleApplier>();
    }

    private void Update()
    {
        if (npcInRange == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            //roleApplier.ApplyRole(npcInRange.GetRole());
            OnRatInteracted?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var npc = other.GetComponentInParent<NPCController>();
        if (npc != null) npcInRange = npc;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var npc = other.GetComponentInParent<NPCController>();
        if (npc != null && npc == npcInRange) npcInRange = null;
    }
}
