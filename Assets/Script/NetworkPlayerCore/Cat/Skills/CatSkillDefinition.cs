using UnityEngine;

[CreateAssetMenu(menuName = "Mousequrade/Cat Skill Definition")]
public class CatSkillDefinition : ScriptableObject
{
    [Header("Id")]
    public BuffCardEffectId buffId;
    public PlayerUsable usable;

    [Header("UI")]
    public string displayName;
    public Sprite icon;
    public int cycleOrder;

    [Header("Cooldown")]
    public float cooldownDuration = 12f;
}