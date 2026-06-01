using UnityEngine;

public class SmokeBombItem : MonoBehaviour, IAOETargetItem
{
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float maxUseDistance = 5f;
    [SerializeField] private ItemUseMode useMode = ItemUseMode.AOETargeted;
    [SerializeField] private LayerMask targetMask;
    public ItemId ItemId => ItemId.SmokeBomb;
    public ItemUseMode UseMode => useMode;

    public float Radius => radius;
    public float MaxUseDistance =>  maxUseDistance;
    public LayerMask TargetMask => targetMask;


    public void UseServer(ItemUseContext context)
    {
        Debug.Log("[SmokeBombItem] Use smoke bomb from server!!");
        //TODO: Implement actual effect of using Smoke Bomb
    }
}