using UnityEngine;

public struct ItemUseContext
{
    public Transform UserTransform;
    public Vector2 UserPosition;
    public ulong UserClientId;

    public ItemUseNetworkRequestData Request;
    public ItemDefinition Definition;
    public int ItemLevel;
}