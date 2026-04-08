using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuffCardButtonHandler : MonoBehaviour
{
    [Header("Buff Config")]
    [SerializeField] private BuffCardEffectId targetEffectId;

    [Header("Role Receiving This Buff")]
    [SerializeField] private PlayerRole targetRole = PlayerRole.Cat;

    [Header("Button Config")]
    [SerializeField] private Button buffSelectButton;


    private bool isProcessingClick;

    private void Awake()
    {
        if (buffSelectButton == null)
        {
            buffSelectButton = GetComponent<Button>();
        }

        if (buffSelectButton == null)
        {
            Debug.LogError("[BuffCardButtonHandler] No Button component assigned or found.", this);
            return;
        }

        buffSelectButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (isProcessingClick) return;
        isProcessingClick = true;

        try
        {
            if (NetworkRoleBuffSystem.Instance != null)
            {
                NetworkRoleBuffSystem.Instance.AcquireEffectForRole(targetRole, targetEffectId);
                Debug.Log($"[{gameObject.name}] Selected buff effect: {targetEffectId} for role: {targetRole}");
            }
            else
            {
                Debug.LogError("[BuffCardButtonHandler] NetworkRoleBuffSystem.Instance was not found.", this);
            }
           
            if (BuffCardSpawnManager.Instance != null &&
                BuffCardSpawnManager.Instance.ActiveChoiceController != null)
            {
                BuffCardSpawnManager.Instance.ActiveChoiceController.NotifyBuffSelectionFinished();
            }
            else
            {
                Debug.LogWarning("[BuffCardButtonHandler] No CatBuffChoiceController found on local client.");
                if (BuffCardSpawnManager.Instance != null)
                {
                    BuffCardSpawnManager.Instance.CloseCurrentPanel();
                }
            }
        }
        finally
        {
            isProcessingClick = false;
        }
    }

    private void OnDestroy()
    {
        if (buffSelectButton != null)
        {
            buffSelectButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}