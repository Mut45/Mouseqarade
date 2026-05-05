
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerRoleState))]
public class PlayerCameraVisionOcclusionController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask visionOccludingLayer;
    [SerializeField] private List<CameraVisionOccluder> currentlyTransparent = new();

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
            Debug.LogWarning("[PlayerCameraVisionOcclusion] No camera found.");
            RestoreAll();
            return;
        }

        // 1. Use Raycast to find the objects that are currently "covering" the player
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 targetPos = transform.position;
        Vector3 direction = targetPos - cameraPos;
        float distance = direction.magnitude;
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            cameraPos,
            direction.normalized,
            distance,
            visionOccludingLayer
        );
        // 2. Render the objects that have the componnent "CameraVisionOccluder"
        HashSet<CameraVisionOccluder> transparentThisTick = new();

        foreach (RaycastHit2D hit in hits)
        {
            CameraVisionOccluder occluder = hit.collider.GetComponentInParent<CameraVisionOccluder>();
            if (occluder == null)
            {
                continue;
            }

            transparentThisTick.Add(occluder);
            if (!currentlyTransparent.Contains(occluder))
            {
                occluder.SetTransparent();
            }
            Debug.Log("[PlayerCameraVisionOcclusion] Found a occluder.");

        }

        foreach (CameraVisionOccluder oldOccluder in currentlyTransparent)
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
        foreach (CameraVisionOccluder newOccluder in transparentThisTick)
        {
                currentlyTransparent.Add(newOccluder);
        }

    }

    private void RestoreAll()
    {
        foreach (CameraVisionOccluder occluder in currentlyTransparent)
        {
            if (occluder != null)
            {
                occluder.Restore();
            }
        }

        currentlyTransparent.Clear();
    }
}