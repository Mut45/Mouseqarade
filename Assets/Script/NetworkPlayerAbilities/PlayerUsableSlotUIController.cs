using System.ComponentModel.Design.Serialization;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// TODO: Future Implementation When the Mouse Skill System is Introduced
// 1. CatSkillController implements IPlayerUsableController
// 2. MouseItemController implements IPlayerUsableController
// 3. PlayerUsableSlotUI reads IPlayerUsableController
public class PlayerUsableSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image overlayImage;

    [Header("Skill overlay alpha values")]
    [SerializeField] private float noSkillAlpha = 0.45f;
    [SerializeField] private float readyAlpha = 0f;
    [SerializeField] private float onCooldownAlpha = 0.65f;

    [Header("Binding parameters")]
    [SerializeField] private float maxBindingDuration = 5f;

    private CatSkillController catSkillController;
    private bool ifBindingStopped = false;
    private float stopBindingTime;
    private void Awake()
    {
        if (visualRoot == null)
        {
            visualRoot = gameObject;
        }

        stopBindingTime = Time.time + maxBindingDuration;

    }
    void Update()
    {
        if (catSkillController == null && !ifBindingStopped)
        {
            TryFindingLocalOwnedPlayer();            
        }
        
        RefreshUIVisual();
    }

    private void TryFindingLocalOwnedPlayer()
    {
        // Due to network loading time difference, we have to keep trying to bind the local cat skill controller.
        // This is set up to optimize this process and not run the binding every frame forever.
        if (Time.time >= stopBindingTime)
        {
            ifBindingStopped = true;
        }
        LocalOwnedPlayerLookUp.TryGetLocalOwnedComponent(out catSkillController);
    }

    private void RefreshUIVisual()
    {
        if (visualRoot == null) return;

        if (catSkillController == null)
        {
            Debug.LogWarning("[PlayerUsableSlotUIController] Can't find the Cat skill controller");
            visualRoot.SetActive(false);
            return;
        }
        visualRoot.SetActive(true);

        CatSkillDefinition currentSkillDefinition = catSkillController.GetCurrentSkillDefinition();
        bool ifHasSkill = currentSkillDefinition != null;

        RefreshBorder();
        RefreshIcon(ifHasSkill, currentSkillDefinition);
        RefreshOverlay(ifHasSkill);
        

    }
    #region Refresh UI Elements
    private void RefreshBorder()
    {
        if (borderImage == null)
        {
            return;
        }

        borderImage.enabled = true;
    }

    private void RefreshIcon(bool hasSkill, CatSkillDefinition currentSkill)
    {
        if (iconImage == null)
        {
            return;
        }

        iconImage.enabled = hasSkill;
        iconImage.sprite = hasSkill ? currentSkill.icon : null;
    }

    private void RefreshOverlay(bool hasSkill)
    {
        if (overlayImage == null)
        {
            return;
        }

        overlayImage.enabled = true;

        if (!hasSkill)
        {
            SetImageAlpha(overlayImage, noSkillAlpha);
            overlayImage.fillAmount = 1f;
            return;
        }

        SetImageAlpha(overlayImage, onCooldownAlpha);

        if (catSkillController.IsCurrentSkillReady())
        {
            overlayImage.fillAmount = 0f;
            return;
        }
        
        overlayImage.fillAmount =
            catSkillController.GetCurrentSkillRemainingCooldownNormalized();

    }
    #endregion

    #region Image
    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
    #endregion

}