using System;
using Unity.Netcode;


[Serializable]
public struct ItemInventoryEntry : INetworkSerializable, IEquatable<ItemInventoryEntry>
{
    public ItemId ItemId;
    public int Count;
    public int Level;

    public ItemInventoryEntry(ItemId itemId, int count, int level)
    {
        ItemId = itemId;
        Count = count;
        Level = level;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ItemId);
        serializer.SerializeValue(ref Count);
        serializer.SerializeValue(ref Level);
    }

    public bool Equals(ItemInventoryEntry other)
    {
        return ItemId == other.ItemId &&
               Count == other.Count &&
               Level == other.Level;
    }
}