using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerRoleState))]
public class CatSkillController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRoleState roleState;

    [Header("Skills")]
    [SerializeField] private CatSkillType[] availableSkills =
    {
        CatSkillType.RedLightGreenLight
    };

    [SerializeField] private float redLightGreenLightCooldown = 12f; // This cooldown starts immediately after the countdown starts, actual time is cd - delay - duration

    private int currentSkillIndex;
    private readonly Dictionary<CatSkillType, double> cooldownEndTimes = new();

    private void Awake()
    {
        if (roleState == null) roleState = GetComponent<PlayerRoleState>();
    }

    private void OnEnable()
    {
        if (roleState != null)
        {
            roleState.OnRoleChanged += HandleRoleChanged;
        }
    }

    private void OnDisable()
    {
        if (roleState != null)
        {
            roleState.OnRoleChanged -= HandleRoleChanged;
        }
    }

    public override void OnNetworkSpawn()
    {
        HandleRoleChanged(roleState != null ? roleState.GetRole() : PlayerRole.Mouse);
    }

    private void HandleRoleChanged(PlayerRole newRole)
    {
        bool isCat = newRole == PlayerRole.Cat;
        enabled = true; 
        currentSkillIndex = Mathf.Clamp(currentSkillIndex, 0, Mathf.Max(0, availableSkills.Length - 1));

        if (!isCat)
        {
        }
    }

    public void HandleCycleSkillInput()
    {
        if (!CanHandleSkillInput()) return;
        if (availableSkills == null || availableSkills.Length == 0) return;

        currentSkillIndex = (currentSkillIndex + 1) % availableSkills.Length;
        Debug.Log($"[CatSkillController] Selected skill: {availableSkills[currentSkillIndex]}");
    }

    public void HandleUseSkillInput()
    {
        if (!CanHandleSkillInput()) return;
        if (availableSkills == null || availableSkills.Length == 0) return;

        CatSkillType selectedSkill = availableSkills[currentSkillIndex];
        if (IsSkillOnCooldown(selectedSkill)) return;

        switch (selectedSkill)
        {
            case CatSkillType.RedLightGreenLight:
                TryUseRedLightGreenLight();
                break;
        }
    }

    private bool CanHandleSkillInput()
    {
        if (!IsOwner) return false;
        if (roleState == null) return false;
        return roleState.GetRole() == PlayerRole.Cat;
    }

    private bool IsSkillOnCooldown(CatSkillType skill)
    {
        if (!cooldownEndTimes.TryGetValue(skill, out double endTime))
            return false;

        double now = NetworkManager.Singleton != null
            ? NetworkManager.Singleton.LocalTime.Time
            : Time.timeAsDouble;

        return now < endTime;
    }

    private void StartLocalCooldown(CatSkillType skill, float duration)
    {
        double now = NetworkManager.Singleton != null
            ? NetworkManager.Singleton.LocalTime.Time
            : Time.timeAsDouble;

        cooldownEndTimes[skill] = now + duration;
    }

    private void TryUseRedLightGreenLight()
    {
        if (NetworkRoleBuffSystem.Instance == null) return;
        Debug.Log("[CatSkillController] TryUseRLGL() called");
        NetworkRoleBuffSystem.Instance.TriggerRedLightGreenLight();
        StartLocalCooldown(CatSkillType.RedLightGreenLight, redLightGreenLightCooldown);
    }
}