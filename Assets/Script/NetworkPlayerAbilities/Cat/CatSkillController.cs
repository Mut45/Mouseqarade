using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerRoleState))]
public class CatSkillController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRoleState roleState;

    [Header("Skill Definitions")]
    [SerializeField] private List<CatSkillDefinition> allSkillDefinitions = new();
    [SerializeField] private readonly List<CatSkillDefinition> availableSkills = new();
    private CatSkillDefinition currentlySelectedSkillDefinition;
    private readonly Dictionary<PlayerUsable, double> cooldownEndTimes = new();

    private void Awake()
    {
        allSkillDefinitions.Sort((a, b) => a.cycleOrder.CompareTo(b.cycleOrder));
    }

    #region Input Handlers
    public void HandleUseSkillInput()
    {
        if (!CanHandleSkillInput()) return;

        CatSkillDefinition currentSkillDefiniton = GetCurrentSkillDefinition();
        if (currentSkillDefiniton == null)
        {
            return;
        }

        if (IsSkillOnCooldown(currentSkillDefiniton))
        {
            return;
        }

        switch (currentSkillDefiniton.usable)
        {
            case PlayerUsable.RedLightGreenLight:
                TryUseRedLightGreenLight(currentSkillDefiniton);
                break;
        }

    }

    public void HandleCycleSkillInput()
    {
        if (!CanHandleSkillInput()) return;

        if (availableSkills.Count == 0)
        {
            currentlySelectedSkillDefinition = null;
            return;
        }

        CatSkillDefinition currentSkillDefiniton = GetCurrentSkillDefinition();
        int currentSkillIndex = availableSkills.IndexOf(currentSkillDefiniton);
        currentSkillIndex = (currentSkillIndex + 1) % availableSkills.Count;
        currentlySelectedSkillDefinition = availableSkills[currentSkillIndex];
    }
    #endregion

    #region Exposed public functions
    public CatSkillDefinition GetCurrentSkillDefinition()
    {
        if (availableSkills.Count == 0)
        {
            currentlySelectedSkillDefinition = null;
            return null;
        }

        if (currentlySelectedSkillDefinition == null || !availableSkills.Contains(currentlySelectedSkillDefinition))
        {
            currentlySelectedSkillDefinition = availableSkills[0];
        }

        return currentlySelectedSkillDefinition;
    }

    public void AddUsableSkill(PlayerUsable usable)
    {
        CatSkillDefinition definition = FindDefinitionForUsable(usable);

        if (definition == null)
        {
            Debug.LogWarning($"[CatSkillController] No definition found for {usable}");
            return;
        }

        if (availableSkills.Contains(definition))
        {
            return;
        }

        availableSkills.Add(definition);
        availableSkills.Sort((a, b) => a.cycleOrder.CompareTo(b.cycleOrder));

        if (currentlySelectedSkillDefinition == null)
        {
            currentlySelectedSkillDefinition = availableSkills[0];
        }
    }

    public bool HasAnySkill()
    {
        return availableSkills.Count > 0;
    }
    
    public bool IsCurrentSkillReady()
    {
        CatSkillDefinition current = GetCurrentSkillDefinition();

        if (current == null)
        {
            return false;
        }

        return !IsSkillOnCooldown(currentlySelectedSkillDefinition);
    }

    public float GetCurrentSkillRemainingCooldownNormalized()
    {
        CatSkillDefinition currentSkill = GetCurrentSkillDefinition();
        if (currentSkill == null)
        {
            return 0f;            
        }

        if (!cooldownEndTimes.TryGetValue(GetCurrentSkillDefinition().usable, out double endTime))
        {
            return 0f;
        }

        double now = NetworkManager.Singleton != null
        ? NetworkManager.Singleton.LocalTime.Time
        : Time.timeAsDouble;
        double remainingTime = endTime - now;

        return Mathf.Clamp01((float)(remainingTime/currentlySelectedSkillDefinition.cooldownDuration));
    }

    #endregion
    
    #region Cooldown management
    private bool IsSkillOnCooldown(CatSkillDefinition skillDefiniton)
    {
        if (!cooldownEndTimes.TryGetValue(skillDefiniton.usable, out double endTime))
            return false;

        double now = NetworkManager.Singleton != null
            ? NetworkManager.Singleton.LocalTime.Time
            : Time.timeAsDouble;

        return now < endTime;
    }
    private void StartLocalCooldown(CatSkillDefinition definition)
    {
        cooldownEndTimes[definition.usable] =
            GetNetworkTime() + definition.cooldownDuration;
    }

    // private void StartLocalCooldown(PlayerUsable skill, float duration)
    // {
    //     double now = NetworkManager.Singleton != null
    //         ? NetworkManager.Singleton.LocalTime.Time
    //         : Time.timeAsDouble;

    //     cooldownEndTimes[skill] = now + duration;
    // }

    #endregion


    #region Network Utility Helpers
    private double GetNetworkTime()
    {
        if (NetworkManager.Singleton != null)
        {
            return NetworkManager.Singleton.LocalTime.Time;
        }

        return Time.timeAsDouble;
    }
    #endregion

    private CatSkillDefinition FindDefinitionForUsable(PlayerUsable usable)
    {
        foreach (CatSkillDefinition skillDefinition in allSkillDefinitions)
        {
            if (skillDefinition == null)
            {
                continue;                
            }

            if (skillDefinition.usable == usable)
            {
                return skillDefinition;
            }
        }

        return null;
    }

    private bool CanHandleSkillInput()
    {
        if (!IsServer) return false;
        if (roleState == null) return false;
        return roleState.GetRole() == PlayerRole.Cat;
    }

//     private void Awake()
//     {
//         if (roleState == null) roleState = GetComponent<PlayerRoleState>();
//     }

//     private void OnEnable()
//     {
//         if (roleState != null)
//         {
//             roleState.OnRoleChanged += HandleRoleChanged;
//         }
//     }

//     private void OnDisable()
//     {
//         if (roleState != null)
//         {
//             roleState.OnRoleChanged -= HandleRoleChanged;
//         }
//     }

//     public override void OnNetworkSpawn()
//     {
//         HandleRoleChanged(roleState != null ? roleState.GetRole() : PlayerRole.Mouse);
//     }

//     private void HandleRoleChanged(PlayerRole newRole)
//     {
//         bool isCat = newRole == PlayerRole.Cat;
//         enabled = true; 
//         currentSkillIndex = Mathf.Clamp(currentSkillIndex, 0, Mathf.Max(0, playerUsables.Length - 1));

//         if (!isCat)
//         {
//         }
//     }

//     public void HandleCycleSkillInput()
//     {
//         if (!CanHandleSkillInput()) return;
//         if (playerUsables == null || playerUsables.Length == 0) return;

//         currentSkillIndex = (currentSkillIndex + 1) % playerUsables.Length;
//         Debug.Log($"[CatSkillController] Selected skill: {playerUsables[currentSkillIndex]}");
//     }

//     public void HandleUseSkillInput()
//     {
//         if (!CanHandleSkillInput()) return;
//         if (playerUsables == null || playerUsables.Length == 0) return;

//         PlayerUsable selectedSkill = playerUsables[currentSkillIndex];
//         if (IsSkillOnCooldown(selectedSkill)) return;

//         switch (selectedSkill)
//         {
//             case PlayerUsable.RedLightGreenLight:
//                 TryUseRedLightGreenLight();
//                 break;
//         }
//     }



//     public PlayerUsable GetCurrentSkill()
//     {
//         List<PlayerUsable> availableSkills = SyncAvailableSkills();

//         if (availableSkills.Count == 0)
//         {
//             return PlayerUsable.None;
//         }

//         currentSkillIndex = Mathf.Clamp(
//             currentSkillIndex,
//             0,
//             availableSkills.Count - 1
//         );

//         return availableSkills[currentSkillIndex];
//     }

//     public float GetCooldownDuration(PlayerUsable skill)
//     {
//         switch (skill)
//         {
//             case PlayerUsable.RedLightGreenLight:
//                 return redLightGreenLightCooldown;
//             default:
//                 return 0f;
//         }
//     }
//     public bool HasAnySkill()
//     {
//         return playerUsables != null && playerUsables.Length > 0;
//     }

//     public bool IsCurrentSkillReady()
//     {
//         PlayerUsable currentSkill = GetCurrentSkill();
        
//         if(currentSkill == PlayerUsable.None)
//         {
//             return false;
//         }

//         return !IsSkillOnCooldown(currentSkill);
//     }

    private void TryUseRedLightGreenLight(CatSkillDefinition skillDefinition)
    {
        if (NetworkRoleBuffSystem.Instance == null) return;
        Debug.Log("[CatSkillController] TryUseRLGL() called");
        NetworkRoleBuffSystem.Instance.TriggerRedLightGreenLight();
        StartLocalCooldown(skillDefinition);
    }
}
