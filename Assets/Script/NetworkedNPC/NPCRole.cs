using UnityEngine;

public enum AxisRule
{
    FourWay,        // Up/Down/Left/Right
    HorizontalOnly, // Left/Right only (crab)
    VerticalOnly,   // Up/Down only
    EightWay        // Includes diagonals
}

[CreateAssetMenu(menuName = "NPC/NPC Role")]
public class NPCRole : ScriptableObject
{
    public string roleName = "Crab";
    public AxisRule movementRule = AxisRule.HorizontalOnly;
    [Min(0.1f)] public float moveSpeed;

    [Header("Wander Settings")]
    [Min(0.1f)] public float minWanderTime = 0.4f;
    [Min(0.1f)] public float maxWanderTime = 1.2f;
    [Range(0f, 1f)] public float idleChance = 0.15f;

    [Header("Visuals")]
    public AnimatorOverrideController animatorOverride;

    public Sprite idleSprite;
}