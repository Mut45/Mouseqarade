using UnityEngine;

[RequireComponent(typeof(PlayerTauntState))]
public class PlayerTauntApplier : MonoBehaviour
{
    [Header("State References")]
    [SerializeField] private PlayerTauntState tauntState;
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private PlayerDisguiseApplier disguiseApplier;

    [Header("Character Visual References")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private RuntimeAnimatorController mouseTauntingAnimController;

    [Header("Dust Visual References")]
    [SerializeField] private GameObject dustVisualRoot;
    [SerializeField] private Animator dustAnimator;
    [SerializeField] private string dustTriggerName = "Play";


    private void Awake()
    {
        if (tauntState == null) tauntState = GetComponent<PlayerTauntState>();
    }

    private void OnEnable()
    {
        if (tauntState == null)
            tauntState = GetComponent<PlayerTauntState>();

        if (tauntState != null)
        {
            tauntState.OnTauntStateChanged += HandleTauntChanged;

            // fallback if the subscription timing is missed because of network lag
            if (tauntState.IsSpawned) HandleTauntChanged(tauntState.GetIsTaunting());
        }
    }

    private void OnDisable()
    {
        if (tauntState != null) tauntState.OnTauntStateChanged -= HandleTauntChanged;
    }

    private void HandleTauntChanged(bool isTaunting)
    {
        if (isTaunting)
            ApplyVisual();
        else
            RestoreVisual();
    }

    private void ApplyVisual()
    {
        PlayDustVisual();
        ApplyMouseTauntingVisual();
    }

    private void PlayDustVisual()
    {
        if (dustVisualRoot != null) dustVisualRoot.SetActive(true);

        if (dustAnimator != null)
        {
            dustAnimator.ResetTrigger(dustTriggerName);
            dustAnimator.SetTrigger(dustTriggerName);
        }
    }

    private void ApplyMouseTauntingVisual()
    {
        if (characterAnimator == null) return;

        if (mouseTauntingAnimController == null) return;

        characterAnimator.runtimeAnimatorController = mouseTauntingAnimController;

    }

    private void RestoreVisual()
    {
        if (dustVisualRoot != null)
            dustVisualRoot.SetActive(false);

        if (disguiseApplier != null)
        {
            disguiseApplier.UpdateDisguiseApplication();
            return;
        }
    }
}