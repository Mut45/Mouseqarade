using Unity.Netcode;
using UnityEngine;

public struct PlayerInputNetworkData : INetworkSerializable
{
    public Vector2 InputDirection;
    public bool PrimaryPressed;
    public bool SecondaryPressed;
    public bool InteractPressed;
    public bool CyclePressed;
    public bool ExtraPressed;
    public void NetworkSerialize<T>(BufferSerializer<T> serailizer) where T : IReaderWriter
    {
        serailizer.SerializeValue(ref InputDirection);
        serailizer.SerializeValue(ref PrimaryPressed);
        serailizer.SerializeValue(ref SecondaryPressed);
        serailizer.SerializeValue(ref InteractPressed);
        serailizer.SerializeValue(ref CyclePressed);
        serailizer.SerializeValue(ref ExtraPressed);
    }
}