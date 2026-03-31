using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(Collider2D))]
public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private List<NetworkNPCController> npcsInRange = new ();
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
        
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("NPC"))
        {
            NetworkNPCController npc = collision.GetComponent<NetworkNPCController>();
            npcsInRange.Remove(npc);
        }
    }

}