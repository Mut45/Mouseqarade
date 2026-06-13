using UnityEngine;
using Unity.Netcode;

public class SmokeBombItem : MonoBehaviour, IAOETargetItem
{
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float maxUseDistance = 5f;
    [SerializeField] private ItemDefinition itemDefinition;
    [SerializeField] private ItemUseMode useMode = ItemUseMode.AOETargeted;
    [SerializeField] private LayerMask targetMask;

    [SerializeField] private GameObject smokeAffectedAreaPrefab;
    public ItemId ItemId => ItemId.SmokeBomb;
    public ItemUseMode UseMode => useMode;

    public float Radius => radius;
    public float MaxUseDistance =>  maxUseDistance;
    public LayerMask TargetMask => targetMask;


    public void UseServer(ItemUseContext context)
    {
        Debug.Log($"[SmokeBombItem] IsServer: {NetworkManager.Singleton.IsServer}, HasTargetPosition: {context.Request.HasTargetPosition}, smokeAffectedArea: {smokeAffectedAreaPrefab == null}");
        if (!NetworkManager.Singleton.IsServer) return;
        if (!context.Request.HasTargetPosition) return;
        if (smokeAffectedAreaPrefab == null) return;
        
        ItemUpgradeData levelData = itemDefinition.GetLevelData(context.ItemLevel);
        if (levelData == null) return;

        GameObject smokeAffectedArea = Instantiate(smokeAffectedAreaPrefab, context.Request.TargetPosition, Quaternion.identity);
        
        NetworkObject smokeAreaNO = smokeAffectedArea.GetComponent<NetworkObject>();
        smokeAreaNO.Spawn(true);

        SmokeAOEAreaController smokeController = smokeAffectedArea.GetComponent<SmokeAOEAreaController>();
        smokeController.StartAOEEffectFromServer
        (
            context.Request.TargetPosition,
            levelData.Radius,
            levelData.Duration
        );

    }
    
    
}