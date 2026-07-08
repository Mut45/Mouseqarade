using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public enum PlayerHandUIState
{
    Default,
    Selecting,
    SpecificInformation
}

/// <summary>
/// Local-only controller for the mouse player's card hand UI.
/// Owns hand states, curved card layout, and individual card UI bindings.
/// </summary>
public class PlayerHandUIController : MonoBehaviour
{
    [Header("Cards")]
    [SerializeField] private MouseItemCardUI[] cardUIs;

    [Header("Input Prompt")]
    [SerializeField] private GameObject inputPromptRoot;

    [Header("Curved Hand Layout")]
    [SerializeField] private float cardSpacing = 95f;
    [SerializeField] private float selectingBaseYOffset = 65f;

    [SerializeField] private float handWidth = 520f;
    [SerializeField] private float foldedY = -100f;
    [SerializeField] private float curveHeight = 45f;
    [SerializeField] private float maxFanAngle = 18f;
    [SerializeField] private float selectedYOffset = 115f;
    [SerializeField] private float foldedScale = 0.85f;
    [SerializeField] private float selectedScale = 1.15f;

    private PlayerHandUIState currentState = PlayerHandUIState.Default;
    private int selectedIndex = -1;
    private MouseItemCardUI selectedCard;
    public PlayerHandUIState State => currentState;

    #region Exposed Public Functions For Updating States
    public void SetState(PlayerHandUIState state)
    {
        currentState = state;
        UpdateInputPromptVisibility();
        UpdateCardsVisual();
    }

    public void SetCurrentlySelected(int index)
    {
        selectedIndex = index;
        UpdateCardsVisual();
        UpdateInputPromptVisibility();
    }

    public void BindCards(List<ItemInventoryEntry> entries, ItemDatabase itemDatabase, int index)
    {
        Debug.Log("[PlayerHandUIController] BindCards called.");
        if (entries == null)
        {
            Debug.LogWarning("[PlayerHandUIController] entries is null.");
            return;
        }

        if (cardUIs == null)
        {
            Debug.LogWarning("[PlayerHandUIController] cardUIs array is null.");
            return;
        }
        Debug.Log("[PlayerHandUIController] entries.Count=" + entries.Count + ", cardUIs.Length=" + cardUIs.Length);


        selectedIndex = index;
        int maxVisibleCount = Mathf.Min(entries.Count, cardUIs.Length);
        Debug.Log("[PlayerHandUIController] maxVisibleCount=" + maxVisibleCount);


        for (int i = 0; i < cardUIs.Length; i++)
        {
            bool isVisible = i < maxVisibleCount;
            cardUIs[i].gameObject.SetActive(isVisible);

            if (!isVisible) continue;

            ItemInventoryEntry entry = entries[i];

            if (itemDatabase == null || !itemDatabase.TryGetDefinition(entry.ItemId, out ItemDefinition definition))
            {
                Debug.LogWarning("[PlayerHandUIController] Disable card at index:" + i);
                cardUIs[i].gameObject.SetActive(false);
                continue;
            }

            cardUIs[i].Bind(definition, entry, definition.Description);
        }

        UpdateCardsVisual();
        UpdateInputPromptVisibility();

    }

    public void CancelSelection()
    {
        if (currentState != PlayerHandUIState.Selecting) return;

        currentState = PlayerHandUIState.Default;
        UpdateCardsVisual();
        UpdateInputPromptVisibility();
    }

    public void ToggleSelectedCardDetails()
    {
        switch (currentState)
        {
            case PlayerHandUIState.Selecting:
                currentState = PlayerHandUIState.SpecificInformation;
                break;
            
            case PlayerHandUIState.SpecificInformation:
                currentState = PlayerHandUIState.Selecting;
                break;
        }

        MouseItemCardUI selected = GetCurrentlySelectedCard();
        if (selected != null)
        {
            selected.StartFakeFlipping();
        }

        UpdateCardsVisual();
        UpdateInputPromptVisibility();

    }
    #endregion

    #region Helpers
    private void UpdateInputPromptVisibility()
    {
        bool isSelecting = currentState == PlayerHandUIState.Selecting;
        if (inputPromptRoot != null)
        {
            inputPromptRoot.SetActive(isSelecting);
        }

        if (cardUIs == null) return;

        for (int i = 0; i < cardUIs.Length; i++)
        {
            if (cardUIs[i] == null) continue;

            bool shouldShowFlipPrompt =
                isSelecting &&
                i == selectedIndex &&
                cardUIs[i].gameObject.activeSelf;

            cardUIs[i].SetFlipInputPromptVisible(shouldShowFlipPrompt);
        }
    }
    private void UpdateCardsVisual()
    {
        // Start from the center calculate the positon of the card relative to x = 0
        int count = GetVisibleCardCount();
        float indexAtCenter = (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float offsetFromCenter = i - indexAtCenter;
            float newX = offsetFromCenter * cardSpacing;
            float maxOffset = Mathf.Max(1f, indexAtCenter);
            float posNormalized = offsetFromCenter / maxOffset;

            float baseY = foldedY;

            if (currentState != PlayerHandUIState.Default)
            {
                baseY += selectingBaseYOffset;
            }
            float newY = baseY - curveHeight * posNormalized * posNormalized;

            float rotationZ = -posNormalized * maxFanAngle;

            float newScale = foldedScale;
            if (i == selectedIndex && currentState != PlayerHandUIState.Default)
            {
                newY += selectedYOffset;
                rotationZ = 0f;
                newScale = selectedScale;
                selectedCard = cardUIs[i];
            }
            cardUIs[i].ApplyPoseInHand(new Vector2(newX, newY), rotationZ, newScale);
        }

        if (selectedCard != null)
        {
            selectedCard.BringToFront();
        }

    }

    private int GetVisibleCardCount()
    {
        int count = 0;
        for (int i = 0; i < cardUIs.Length; i++)
        {
            if (cardUIs[i] != null && cardUIs[i].gameObject.activeSelf)
            {
                count++;
            }
        }
        return count;
    }

    private MouseItemCardUI GetCurrentlySelectedCard()
    {
        if (selectedIndex < 0) return null;
        if (selectedIndex >= cardUIs.Length) return null;

        MouseItemCardUI selectedCard = cardUIs[selectedIndex];
        if (selectedCard == null) return null;

        return selectedCard;
    }


    #endregion

}
