using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// For connecting Player Networked Object to the local UI elements
/// </summary>
public class PlayerLocalUIBinder : MonoBehaviour
{
    [Header("Bound Networked Player Componenets")]
    [SerializeField] private PlayerRoleState boundRoleState;
    [SerializeField] private MouseItemController boundMouseItemController;

    [Header("Mouse UI")]
    [SerializeField] private GameObject mousePlayerHUD;
    [SerializeField] private PlayerHandUIController mouseHandUI;


    [Header("Cat UI")]
    [SerializeField] private GameObject catPlayerHUD;

    void Update()
    {
        if (boundRoleState != null) return;

        TryBindLocalPlayer();
    }

    private void OnDisable()
    {
        if (boundRoleState != null)
        {
            boundRoleState.OnRoleChanged -= HandleRoleChanged;
        }
    }

    private void TryBindLocalPlayer()
    {
        // TODO: Add cat player UI control later
        if (!LocalOwnedPlayerLookUp.TryGetLocalOwnedComponent(out PlayerRoleState roleState))
        {
            return;
        }

        boundRoleState = roleState;
        
        if (!LocalOwnedPlayerLookUp.TryGetLocalOwnedComponent(out MouseItemController itemController))
        {
            return;
        }
        boundMouseItemController = itemController;

        //bool isLocalMouse = boundRoleState.GetRole() == PlayerRole.Mouse;
        //Debug.Log($"[PlayerLocalBinder] Find the mouse player is local: {isLocalMouse}");
        InitPlayerHUDElements();
        boundRoleState.OnRoleChanged += HandleRoleChanged;

        if (boundMouseItemController != null)
        {
            boundMouseItemController.SetHandUIController(mouseHandUI);
        }
    }
    
    private void InitPlayerHUDElements()
    {
        bool isLocalMouse = boundRoleState.GetRole() == PlayerRole.Mouse;

        switch (boundRoleState.GetRole())
        {
            case PlayerRole.Mouse:
                if (mousePlayerHUD != null)
                {
                    mousePlayerHUD.SetActive(isLocalMouse);
                }
                if (mouseHandUI != null)
                {
                    mouseHandUI.gameObject.SetActive(isLocalMouse);
                }
                break;
            case PlayerRole.Cat:
                if (catPlayerHUD != null)
                {
                    catPlayerHUD.SetActive(!isLocalMouse);
                }
                break;
        }
    }

    private void HandleRoleChanged(PlayerRole role)
    {
        InitPlayerHUDElements();
    }

}