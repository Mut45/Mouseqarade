
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerRoleState))]
public class PlayerCameraVisionOcclusionFaderController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask visionOccludingLayer;
    [SerializeField] private List<CameraVisionOccluderFader> currentlyTransparent = new();

    void OnDisable()
    {
        RestoreAll();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        if (roleState == null) return;
        if (roleState.GetRole() != PlayerRole.Cat)
        {
            RestoreAll();
            return;
        }

        MakeTransparentObjectCoveringPlayer();

    }

    private void MakeTransparentObjectCoveringPlayer()
    {
        if (mainCamera == null)
        {
            RestoreAll();
            return;
        }

        // 1. Use Raycast to find the objects that are currently "covering" the player
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 targetPos = transform.position;
        Vector3 direction = targetPos - cameraPos;
        float distance = direction.magnitude;
        RaycastHit[] hits = Physics.RaycastAll(
            cameraPos,
            direction.normalized,
            distance,
            visionOccludingLayer
        );

        // 2. Render the objects that have the componnent "CameraVisionOccluderFader"
        HashSet<CameraVisionOccluderFader> transparentThisTick = new();

        foreach (RaycastHit hit in hits)
        {
            CameraVisionOccluderFader occluder = hit.collider.GetComponentInParent<CameraVisionOccluderFader>();
            if (occluder == null)
            {
                continue;
            }

            transparentThisTick.Add(occluder);
            if (!currentlyTransparent.Contains(occluder))
            {
                occluder.SetTransparent();
            }

        }

        foreach (CameraVisionOccluderFader oldOccluder in currentlyTransparent)
        {
            if (oldOccluder == null)
            {
                continue;
            }

            if (!transparentThisTick.Contains(oldOccluder))
            {
                oldOccluder.Restore();
            }
        }

        currentlyTransparent.Clear();
        foreach (CameraVisionOccluderFader newOccluder in transparentThisTick)
        {
                currentlyTransparent.Add(newOccluder);
        }

    }

    private void RestoreAll()
    {
        foreach (CameraVisionOccluderFader occluder in currentlyTransparent)
        {
            if (occluder != null)
            {
                occluder.Restore();
            }
        }

        currentlyTransparent.Clear();
    }

}