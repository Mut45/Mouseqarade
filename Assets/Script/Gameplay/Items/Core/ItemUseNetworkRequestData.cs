using Unity.Netcode;
using UnityEngine;

public struct ItemUseNetworkRequestData : INetworkSerializable
{
    public ItemId ItemId;
    public bool HasTargetPosition;
    public Vector2 TargetPosition;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ItemId);
        serializer.SerializeValue(ref HasTargetPosition);
        serializer.SerializeValue(ref TargetPosition);
    }

}
