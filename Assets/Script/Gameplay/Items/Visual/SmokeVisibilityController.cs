using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;
/// <summary>
/// (Might be implemented in the future)Local-only visual controller to control what players can see depending on their position to the smoke
/// </summary>

public class SmokeVisibilityController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Image overlayImage;

    [Header("Visual Settings")]
    [SerializeField] private float darkAlpha = 0.92f;
    [SerializeField] private float fadeSpeed = 8f;
    [SerializeField] private float edgeSoftness = 0.04f;

    [Header("Debug Only")]
    [SerializeField] private bool debugAlwaysShow = true;
    [SerializeField] private Vector2 debugSmokeCenter = Vector2.zero;
    [SerializeField] private float debugSmokeRadius = 5f;

    private static readonly List<SmokeAOEAreaController> smokeAreas = new();
    private Material material;
    private float currentAlpha;
    private SmokeAOEAreaController currentSmoke;
    private readonly Vector4[] circles = new Vector4[8];

    private static readonly int CircleCenterId = Shader.PropertyToID("_CircleCenter");
    private static readonly int CircleRadiusId = Shader.PropertyToID("_CircleRadius");
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly int SoftnessId = Shader.PropertyToID("_Softness");
    
    
    public static void Register(SmokeAOEAreaController smoke)
    {
        if (smoke != null && !smokeAreas.Contains(smoke))
        {
            smokeAreas.Add(smoke);
        }
    }

    public static void Unregister(SmokeAOEAreaController smoke)
    {
        if (smoke != null)
        {
            smokeAreas.Remove(smoke);
        }
    }

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (overlayImage != null)
        {
            material = Instantiate(overlayImage.material);
            overlayImage.material = material;
        }

    }

    private void Update()
    {
        if (material == null || targetCamera == null) return;

        if (debugAlwaysShow)
        {
            TestDarknessFadeIn(debugSmokeCenter, debugSmokeRadius);
            return;
        }
        
        Transform localPlayer = GetLocalPlayerTransform();
        if (localPlayer == null)
        {
            DarknessFadeOut();
            return;
        }

        currentSmoke = FindSmokeContainingPlayer(localPlayer.position);

        if (currentSmoke == null)
        {
            DarknessFadeOut();
            return;
        }

        DarknessFadeIn(currentSmoke);
    }

    #region Darkness Fade in/Fade Out
    private void DarknessFadeIn(SmokeAOEAreaController smoke)
    {
        currentAlpha = Mathf.MoveTowards(
            currentAlpha,
            darkAlpha,
            fadeSpeed * Time.deltaTime
        );
        Vector3 screenCenter = targetCamera.WorldToViewportPoint(smoke.Center);

        Vector3 worldEdge = new Vector3(
            smoke.Center.x + smoke.Radius,
            smoke.Center.y,
            0f
        );

        Vector3 screenEdge = targetCamera.WorldToViewportPoint(worldEdge);
        float screenRadius = Vector2.Distance(screenCenter, screenEdge);

        material.SetVector(CircleCenterId, screenCenter);
        material.SetFloat(CircleRadiusId, screenRadius);
        material.SetFloat(SoftnessId, edgeSoftness);
        material.SetFloat(AlphaId, currentAlpha);
    }

    private void TestDarknessFadeIn(Vector2 worldCenter, float worldRadius)
    {
        currentAlpha = Mathf.MoveTowards(
            currentAlpha,
            darkAlpha,
            fadeSpeed * Time.deltaTime
        );

        Vector3 screenCenter = targetCamera.WorldToViewportPoint(
            new Vector3(worldCenter.x, worldCenter.y, 0f)
        );

        Vector3 worldEdge = new Vector3(
            worldCenter.x + worldRadius,
            worldCenter.y,
            0f
        );

        Vector3 screenEdge = targetCamera.WorldToViewportPoint(worldEdge);
        float screenRadius = Vector2.Distance(screenCenter, screenEdge);

        material.SetVector(CircleCenterId, screenCenter);
        material.SetFloat(CircleRadiusId, screenRadius);
        material.SetFloat(SoftnessId, edgeSoftness);
        material.SetFloat(AlphaId, currentAlpha);
    }

    private void DarknessFadeOut()
    {
        currentAlpha = Mathf.MoveTowards(
            currentAlpha,
            0f,
            fadeSpeed * Time.deltaTime
        );

        material.SetFloat(AlphaId, currentAlpha);
    }
    #endregion

    #region Helpers
    private Transform GetLocalPlayerTransform()
    {
        if (NetworkManager.Singleton == null) return null;
        if (NetworkManager.Singleton.LocalClient == null) return null;
        if (NetworkManager.Singleton.LocalClient.PlayerObject == null) return null;

        return NetworkManager.Singleton.LocalClient.PlayerObject.transform;
    }

    private SmokeAOEAreaController FindSmokeContainingPlayer(Vector3 playerPosition)
    {
        SmokeAOEAreaController[] smokes = FindObjectsByType<SmokeAOEAreaController>(FindObjectsSortMode.None);
        
        foreach (SmokeAOEAreaController smoke in smokes)
        {
            if (smoke != null && smoke.Contains(playerPosition))
            {
                return smoke;
            }
        }

        return null;
    }
    #endregion

}