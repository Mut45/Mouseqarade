using System.Drawing;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Should be called on Host branch only.
/// Storing network sync'ed states for the AOE lasting effects.
/// </summary>

[RequireComponent(typeof(NetworkObject))]
public class NetworkedAOEAreaBaseController : NetworkBehaviour
{
    protected NetworkVariable<Vector2> center = new (
        Vector2.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    protected NetworkVariable<float> radius = new (
        0f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    protected NetworkVariable<double> despawnTime = new (
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        UpdateVisualScaleAndPos();

        center.OnValueChanged += HandleCenterChanged;
        radius.OnValueChanged += HandleRadiusChanged;
    }

    public override void OnNetworkDespawn()
    {
        center.OnValueChanged -= HandleCenterChanged;
        radius.OnValueChanged -= HandleRadiusChanged;
    }

    public Vector2 Center => center.Value;
    public float Radius => radius.Value;

    protected virtual void Update()
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton.ServerTime.Time >= despawnTime.Value)
        {
            NetworkObject.Despawn(true);
        }
    }

    #region Handlers
    private void HandleCenterChanged(Vector2 prevValue, Vector2 currValue)
    {
        transform.position = currValue;
    }

    private void HandleRadiusChanged(float prevValue, float currValue)
    {
        UpdateVisualScaleAndPos();
    }
    #endregion

    public void StartAOEEffectFromServer(Vector2 areaCenter, float areaRadius, float duration)
    {
        if (!IsServer) return;

        center.Value = areaCenter;
        radius.Value = areaRadius;
        despawnTime.Value = NetworkManager.Singleton.ServerTime.Time + duration;
        UpdateVisualScaleAndPos();
    }

    protected virtual void UpdateVisualScaleAndPos()
    {
        transform.localScale = Vector3.one * radius.Value;
        // Vector3 originalScale = transform.localScale;
        // transform.localScale = originalScale * radius.Value;
        transform.position = new Vector3(center.Value.x, center.Value.y, transform.position.z);
    }


    public bool Contains(Vector3 pos)
    {
        return Vector3.Distance(pos, center.Value) <= radius.Value;
    }


}