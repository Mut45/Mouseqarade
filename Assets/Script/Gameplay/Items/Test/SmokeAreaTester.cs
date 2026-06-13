using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class SmokeAOEAreaDebugTester : MonoBehaviour
{
    [SerializeField] private SmokeAOEAreaController smokeArea;
    [SerializeField] private Vector2 areaCenter = Vector2.zero;
    [SerializeField] private float areaRadius = 10;
    [SerializeField] private float duration = 10000;
    [SerializeField] private Transform fakePlayer;

    private void Start()
    {

    }
    private void Update()
    {
        if (NetworkManager.Singleton == null) return;
        if (!NetworkManager.Singleton.IsServer) return;
        if (smokeArea == null || fakePlayer == null) return;

        smokeArea.UpdateLocalBlockerVisibility(fakePlayer.position);
    }
}