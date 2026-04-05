using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class CatBuffCycleCoordinator : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkTimeManager timeManager;

    private void Awake()
    {
        if (timeManager == null)
        {
            timeManager = FindAnyObjectByType<NetworkTimeManager>();
        }
    }

    private void OnEnable()
    {
        if (timeManager != null)
        {
            timeManager.OnEveryCycleElapsed += HandleCycleElapsed;
        }
    }

    private void OnDisable()
    {
        if (timeManager != null)
        {
            timeManager.OnEveryCycleElapsed -= HandleCycleElapsed;
        }
    }

    private void HandleCycleElapsed()
    {
        if (!IsServer) return;

        CatBuffChoiceController catChoiceController = FindCatChoiceController();
        if (catChoiceController == null)
        {
            Debug.LogWarning("[CatBuffCycleCoordinator] No cat player found.");
            return;
        }

        catChoiceController.BeginBuffChoiceFromServer();
    }

    private CatBuffChoiceController FindCatChoiceController()
    {
        CatBuffChoiceController[] controllers =
            FindObjectsByType<CatBuffChoiceController>(FindObjectsSortMode.None);

        foreach (CatBuffChoiceController controller in controllers)
        {
            if (controller == null) continue;

            PlayerRoleState roleState = controller.GetComponent<PlayerRoleState>();
            if (roleState == null) continue;

            if (roleState.GetRole() == PlayerRole.Cat)
            {
                return controller;
            }
        }

        return null;
    }
}