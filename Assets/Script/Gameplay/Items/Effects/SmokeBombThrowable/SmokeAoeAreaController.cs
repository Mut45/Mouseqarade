using UnityEngine;

public class SmokeAOEAreaController : NetworkedAOEAreaBaseController
{
    [Header("Local Smoke Blocking")]
    [SerializeField] private GameObject viewBlockerSphere;
    
    private bool wasBlockerActive = true;

    protected override void Update()
    {
        base.Update();

        Transform localPlayer = ItemHelpers.GetLocalPlayerTransform();
        if (localPlayer == null) return;

        UpdateLocalBlockerVisibility(localPlayer.position);
    }

    public void UpdateLocalBlockerVisibility(Vector3 localPlayerPos)
    {
        if (viewBlockerSphere == null) return;
        
        bool ifLocalPlayerInSmoke = Contains(localPlayerPos);
        Debug.Log($"[SmokeAreaController] Is the local player in smoke: {ifLocalPlayerInSmoke}");
        if (wasBlockerActive == !ifLocalPlayerInSmoke) return;

        viewBlockerSphere.SetActive(!ifLocalPlayerInSmoke);
        wasBlockerActive = !ifLocalPlayerInSmoke;
    }

}