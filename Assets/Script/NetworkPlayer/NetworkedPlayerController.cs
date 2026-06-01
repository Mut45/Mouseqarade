using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(PlayerMovement))]
public class NetworkedPlayerController : NetworkBehaviour
{
    [Header("Player Movement & Ability Reference")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseItemController mouseItem;
    [SerializeField] private MouseAbilityController mouseAbility;
    [SerializeField] private CatAbilityController catAbility;
    [SerializeField] private CatSkillController catSkill;
    [SerializeField] private PlayerRoleState roleState;

    private NetworkVariable<bool>  syncedMovementLock = new (
        false,
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<PlayerInputNetworkData> syncedInputData = new (
        default, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    private PlayerInputNetworkData prevLocalInputData;
    private void Awake()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (catSkill == null) catSkill = GetComponent<CatSkillController>();
    }

    public override void OnNetworkSpawn()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        syncedInputData.OnValueChanged += HandleSyncedInputChanged;
        syncedMovementLock.OnValueChanged += HandleMovementLockChanged;
    }

    public override void OnNetworkDespawn()
    {
        syncedInputData.OnValueChanged -= HandleSyncedInputChanged;
        syncedMovementLock.OnValueChanged -= HandleMovementLockChanged;
    }

    private void Update()
    {
        if (IsOwner)
        {
            PlayerInputNetworkData inputData = ReadLocalInput();
            HandleOwnerLocalInpupt(prevLocalInputData, inputData);

            if (IsServer)
            {
                syncedInputData.Value = inputData;
            } 
            else
            {
                SubmitInputDataViaServerRpc(inputData);    
            }

            prevLocalInputData = inputData;
        }

        playerMovement.UpdateVisuals(syncedInputData.Value.InputDirection);

    }

    public void SetMovementLockFromServer(bool locked)
    {
        if (IsServer)
        {
            syncedMovementLock.Value = locked;
        }
    }

    #region Movement Lock Request Flow
    public void RequestMovementLock(bool locked)
    {
        if (!IsOwner) return;

        if (IsServer)
        {
            SetMovementLockFromServer(locked);
        }
        else
        {
            RequestMveomentLockServerRpc(locked);
        }
    }

    [ServerRpc]
    private void RequestMveomentLockServerRpc(bool locked)
    {
        SetMovementLockFromServer(locked);
    }
    #endregion

    private void HandleMovementLockChanged(bool prevValue, bool currValue)
    {
        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(currValue);            
        }
    }

    private void HandleOwnerLocalInpupt(PlayerInputNetworkData prevInput, PlayerInputNetworkData currInput)
    {
        if (!IsOwner) return;

        if (roleState == null) return;

        switch (roleState.GetRole())
        {
            case PlayerRole.Mouse:
                if (mouseItem != null)
                {
                    mouseItem.HandleLocalInput(prevInput, currInput);
                }
                break;
            case PlayerRole.Cat:
                if (catSkill != null)
                {
                    catSkill.HandleLocalInput(prevInput, currInput);
                }
                break;
        }
    }   
    private void HandleSyncedInputChanged(PlayerInputNetworkData prevInput, PlayerInputNetworkData currInput)
    {
        //Debug.Log("[Network] Synced input data changed.");
        if (!IsServer) return;

        switch (roleState.GetRole())
        {
            case PlayerRole.Mouse:
                mouseAbility.HandleInput(prevInput, currInput);
                break;
            case PlayerRole.Cat:
                //TODO: Implement cat input handler
                catAbility.HandleInput(prevInput, currInput);
                break;
        }
        
    }

    void FixedUpdate()
    {
        if (!IsServer) return;

        playerMovement.SetMovementInput(syncedInputData.Value.InputDirection);
        playerMovement.Move(Time.fixedDeltaTime);
    }

    private PlayerInputNetworkData ReadLocalInput()
    {
        PlayerInputNetworkData inputData = new PlayerInputNetworkData();
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector2 inputDirection = new Vector2(horizontalInput, verticalInput);
        // diagonal input should not be more than the horizontal or the vertical movement
        if (inputDirection.sqrMagnitude > 1f) inputDirection = inputDirection.normalized;

        inputData.InputDirection = inputDirection;
        inputData.PrimaryPressed = Input.GetKeyDown(KeyCode.Space);
        inputData.CyclePressed = Input.GetKeyDown(KeyCode.Q); // cycle skill/item
        inputData.SecondaryPressed = Input.GetKeyDown(KeyCode.F); // use selected skill/item
        inputData.InteractPressed = Input.GetKeyDown(KeyCode.E); // interact with environment
        inputData.ExtraPressed = Input.GetKeyDown(KeyCode.T); // extra key pressed for action such as taunting
        

        return inputData; 
    }

    [ServerRpc]
    private void SubmitInputDataViaServerRpc(PlayerInputNetworkData inputData)
    {
        if (inputData.InputDirection.sqrMagnitude > 1.0f) 
            inputData.InputDirection = inputData.InputDirection.normalized;

        syncedInputData.Value = inputData;
    }
}