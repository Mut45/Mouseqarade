using UnityEngine;

[System.Serializable]
public class ItemUpgradeData
{
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float maxUseDistance = 5f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float power = 1f;
    [SerializeField] private GameObject useVfxPrefab;
    [SerializeField] private GameObject impactVFXPrefab;
    public float Radius => radius;
    public float MaxUseDistance => maxUseDistance;
    public LayerMask TargetMask => targetMask;
    public float Duration => duration;
    public float Power => power;
    public GameObject UseVFXPrefab => useVfxPrefab;
    public GameObject ImpactVFXPrefab => impactVFXPrefab;
}