using System;
using System.Collections.Generic;
using Unity.Multiplayer.PlayMode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private List<NetworkNPCController> npcsInRange = new ();
    [SerializeField] private NetworkedPlayerController currentPlayerTarget;
    [SerializeField] private NetworkClockController currentClockTarget;
    public NetworkedPlayerController GetCurrentPlayerTarget()
    {
        return currentPlayerTarget;
    }

    public NetworkedPlayerController GetCurrentTargetPlayer()
    {
        return currentPlayerTarget;
    }

    public bool TryGetCurrentClockTarget(out NetworkClockController targetClock)
    {
        targetClock = currentClockTarget;
        return targetClock != null;
    }

    public bool TryGetCurrentNpcTarget(out NetworkNPCController targetNPC)
    {
        targetNPC = null;
        npcsInRange.RemoveAll(npc => npc == null);
        
        if (npcsInRange.Count == 0) return false;

        targetNPC = npcsInRange[0];
        return true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("NPC"))
        {
            NetworkNPCController npc = collision.GetComponent<NetworkNPCController>();
            if (!npcsInRange.Contains(npc))
            {
                npcsInRange.Add(npc);
            }
        }

        if (collision.CompareTag("Player"))
        {
            NetworkedPlayerController player = collision.GetComponent<NetworkedPlayerController>();
            if (currentPlayerTarget == null)
            {
                currentPlayerTarget = player;
            }
        }

        if (collision.CompareTag("Clock"))
        {
            NetworkClockController networkClock = collision.GetComponent<NetworkClockController>();
            if (networkClock != null && currentClockTarget == null)
            {
                currentClockTarget = networkClock;
            }
        }
        
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("NPC"))
        {
            NetworkNPCController npc = collision.GetComponent<NetworkNPCController>();
            npcsInRange.Remove(npc);
        }

        if (collision.CompareTag("Player"))
        {
            NetworkedPlayerController player = collision.GetComponent<NetworkedPlayerController>();
            if (currentPlayerTarget == player)
            {
                currentPlayerTarget = null;
            }
            
        }

        if (collision.CompareTag("Clock"))
        {
            NetworkClockController clock = collision.GetComponent<NetworkClockController>();
            if (currentClockTarget == clock)
            {
                currentClockTarget = null;
            }
        }
    }
}