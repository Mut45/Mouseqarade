using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(PlayerMovement))]
public class NetworkedPlayerController : NetworkBehaviour
{
    [Header("Player Movement & Ability Reference")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseAbilityController mouseAbility;
    [SerializeField] private CatAbilityController catAbility;
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
    private void Awake()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
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
            if (IsServer)
            {
                syncedInputData.Value = inputData;
            } 
            else
            {
                SubmitInputDataViaServerRpc(inputData);    
            }
        }

        playerMovement.UpdateVisuals(syncedInputData.Value.InputDirection);

    }

    public void SetMovementLock(bool locked)
    {
        if (IsServer)
        {
            syncedMovementLock.Value = locked;
        }
    }

    private void HandleMovementLockChanged(bool prevValue, bool currValue)
    {
        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(currValue);            
        }
    }

    private void HandleSyncedInputChanged(PlayerInputNetworkData prevInput, PlayerInputNetworkData currInput)
    {
        Debug.Log("[Network] Synced input data changed.");
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
        inputData.InteractPressed = Input.GetKeyDown(KeyCode.E);
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