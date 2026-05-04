using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


// TODO: Refactor this to be a base interface class when the mouse items system is introduced later
// public class PlayerUsableSlotState : NetworkBehaviour
// {
//     public event Action OnSlotChanged;
//     [SerializeField] private List<PlayerUsableDefinition> ownedUsables = new();
//     private int selectedIndex = -1;
//     private readonly Dictionary<PlayerUsable, float> playerUsableToCooldownDict = new();

//     private void Update()
//     {
//         bool changed = false;

//         List<PlayerUsable> keys = new List<PlayerUsable>(playerUsableToCooldownDict.Keys);

//         foreach (PlayerUsable usableId in keys)
//         {
//             float remaining = playerUsableToCooldownDict[usableId];

//             if (remaining <= 0f) continue;

//             remaining -= Time.deltaTime;

//             if (remaining < 0f)
//             {
//                 remaining = 0f;
//             }

//            playerUsableToCooldownDict[usableId] = remaining;
//             changed = true;
//         }

//         if (changed)
//         {
//             // optimization flag, only invoke the action if any change was applied during this frame
//             OnSlotChanged?.Invoke();
//         }
//     }

//     public void AddUsable(PlayerUsableDefinition usable)
//     {
//         if (usable == null) return;
//         if (ownedUsables.Contains(usable)) return;

//         ownedUsables.Add(usable);
//         if (selectedIndex < 0) selectedIndex = 0;

//         if (!playerUsableToCooldownDict.ContainsKey(usable.id))
//         {
//             playerUsableToCooldownDict.Add(usable.id, 0f);
//         }
//         OnSlotChanged?.Invoke();
//     }

//     public void Cycle()
//     {
//         if (ownedUsables.Count == 0) return;

//         selectedIndex ++;
//         if (selectedIndex >= ownedUsables.Count) selectedIndex = 0;
//         OnSlotChanged?.Invoke();
//     }

//     // public bool IsSelectedUsableReady()
//     // {
//     //     PlayerUsableDefinition selected = GetCurrentSelectedUsable();
//     //     if (selected == null) return false;

//     //     return GetCooldownRemaining(selected.id) <= 0;
//     // }

//     public float GetCooldownRemaining(PlayerUsable id)
//     {
//         if (!playerUsableToCooldownDict.TryGetValue(id, out float remaining))
//         {
//             return 0f;
//         }

//         return remaining;
//     }

//     public PlayerUsableDefinition GetCurrentSelectedUsable()
//     {
//         if (ownedUsables.Count == 0) return null;
//         if (selectedIndex < 0 || selectedIndex >= ownedUsables.Count) return null;

//         return ownedUsables[selectedIndex];
//     }

//     // public void InitSelectedCooldown()
//     // {
//     //     PlayerUsableDefinition selected = GetCurrentSelectedUsable();
//     //     if (selected == null) return;

//     //     playerUsableToCooldownDict[selected.id] = selected.cooldown;
//     //     OnSlotChanged?.Invoke();
//     // }

//     // public void StartCooldown(PlayerUsable usable, float duration)
//     // {
//     //     playerUsableToCooldownDict[usable] = duration;
//     //     OnSlotChanged?.Invoke();
//     // }

// }