using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using Unity.IO.LowLevel.Unsafe;

public class PlayerVirtualCameraBinder : NetworkBehaviour
{
    private CinemachineVirtualCamera virtualCamera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        virtualCamera = FindFirstObjectByType<CinemachineVirtualCamera>();

        if (virtualCamera != null)
        {
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }
    }
}
