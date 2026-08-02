using UnityEngine;

[CreateAssetMenu(
    menuName = "Game/Cat Progression/EXP Reward Definition",
    fileName = "CatExpReward_")]
public class CatExpRewardDefinition : ScriptableObject
{
    [SerializeField] private CatExpRewardEventType eventType;
    [Min(0)]
    [SerializeField] private int expAmount;

    [Header("Optional Presentation Metadata")]
    [SerializeField] private string displayName;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private AudioClip sfx;

    public CatExpRewardEventType EventType => eventType;
    public int ExpAmount => expAmount;
    public string DisplayName => displayName;
    public string Description => description;
    public GameObject VfxPrefab => vfxPrefab;
    public AudioClip Sfx => sfx;
}