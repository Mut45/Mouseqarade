using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// Local-only UI controller for mouse player's usable cards
/// </summary>
public class MouseItemCardUI : MonoBehaviour
{
    [Header("UI Elements Reference")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject cardFront;
    [SerializeField] private GameObject cardBack;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject flipInputPrompt;

    [Header("Parameters")]
    [SerializeField] private float flipDuration;

    private bool isFlipped;
    private Coroutine flipRoutine;

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        ShowFront();
        SetFlipInputPromptVisible(false);
    }

    #region Exposed Public Functions For Visual Controls
    public void ShowFront()
    {
        isFlipped = false;
        cardFront.SetActive(true);
        cardBack.SetActive(false);
    }

    private void ShowBack()
    {
        isFlipped = true;
        cardFront.SetActive(false);
        cardBack.SetActive(true);
    }

    public void StartFakeFlipping()
    {
        if (flipRoutine != null) StopCoroutine(flipRoutine);

         flipRoutine = StartCoroutine(FakeFlipRoutine());
    }

    public void Bind(ItemDefinition definition, ItemInventoryEntry entry, string detailsText)
    {
        if (definition == null) return;

        titleText.text = definition.DisplayName;
        iconImage.sprite = definition.Icon;
        iconImage.enabled = definition.Icon != null;
        descriptionText.text = detailsText;
        countText.text = entry.Count.ToString();

        ShowFront();
        SetFlipInputPromptVisible(false);
    }

    public void ApplyPoseInHand(Vector2 anchoredPosition, float rotationZ, float scale)
    {
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        rectTransform.localScale = new Vector3(scale, scale, 1f);
    }

    public void SetFlipInputPromptVisible(bool visible)
    {
        if (flipInputPrompt != null)
        {
            flipInputPrompt.SetActive(visible);
        }
    }

    public void BringToFront()
    {
        transform.SetAsLastSibling();
    }
    #endregion

    #region Fake Flip Visual
    private IEnumerator FakeFlipRoutine()
    {
        // Change scale on x from 1 to 0 and then 0 to 1. 
        // Swap card face when reaching 0

        float halfDuration = flipDuration * 0.5f;
        Vector3 startScale = rectTransform.localScale;
        Vector3 flatScale = new Vector3(0f, startScale.y, startScale.z);

        for (float t = 0f; t < halfDuration; t += Time.unscaledDeltaTime)
        {
            float p = t / halfDuration;
            rectTransform.localScale = Vector3.Lerp(startScale, flatScale, p);
            yield return null;
        }

        rectTransform.localScale = flatScale;

        if (isFlipped) ShowFront();
        else ShowBack();

        Vector3 endScale = new Vector3(startScale.x, startScale.y, startScale.z);

        for (float t = 0f; t < halfDuration; t += Time.unscaledDeltaTime)
        {
            float p = t / halfDuration;
            rectTransform.localScale = Vector3.Lerp(flatScale, endScale, p);
            yield return null;
        }

        rectTransform.localScale = endScale;
        flipRoutine = null;
    }
    #endregion
}
