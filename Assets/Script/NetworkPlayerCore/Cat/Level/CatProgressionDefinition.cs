using UnityEngine;

[CreateAssetMenu(
    menuName = "Game/Cat Progression/Progression Definition",
    fileName = "CatProgressionDefinition")]
public class CatProgressionDefinition : ScriptableObject
{
    [Tooltip(
        "EXP required to advance from each level. " +
        "Index 0 represents level 1 → 2.")]
    [SerializeField] private int[] expRequiredPerLevel =
    {
        100, // Level 1 → 2
        150, // Level 2 → 3
        225, // Level 3 → 4
        300  // Level 4 → 5
    };

    public int MaxLevel => expRequiredPerLevel.Length + 1;

    public int GetExpRequiredForLevel(int currentLevel)
    {
        if (currentLevel < 1 || currentLevel >= MaxLevel)
            return 0;

        return expRequiredPerLevel[currentLevel - 1];
    }
}