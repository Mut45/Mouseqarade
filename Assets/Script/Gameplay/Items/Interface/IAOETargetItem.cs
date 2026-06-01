using UnityEngine;
using Unity.Netcode;

public interface IAOETargetItem : IUsableItem
{
    float Radius{get;}
    float MaxUseDistance{get;}
    LayerMask TargetMask{get;}
}