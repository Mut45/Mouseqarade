using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemDefinition",
    menuName = "Mousequrade/Items/Item Definition"
)]
public class ItemDefinition : ScriptableObject
{
    [Header("Id")]
    [SerializeField] ItemId id;

    [Header("UI")]
    [SerializeField] private string displayName;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    [Header("Use")]
    [SerializeField] private ItemUseMode useMode;
    

    [Header("Stats")]
    [SerializeField] private int maxStack;
    [SerializeField ] private int cycleOrder = 0;

    [Header("Upgrade Levels")]
    [SerializeField] private ItemUpgradeData[] levels;

    public ItemId Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public ItemUseMode UseMode => useMode;
    public int MaxLevel => levels != null ? levels.Length : 0;
    public int MaxStack => maxStack;
    public int CycleOrder => cycleOrder;
    

    public ItemUpgradeData GetLevelData(int level)
    {
        if (levels == null || levels.Length == 0 || level < 0 || level >= levels.Length)
        {
            Debug.LogWarning($"[ItemDefinition] {name} has no upgrade level data.");
            return null;
        }

        return levels[level];
    }
}