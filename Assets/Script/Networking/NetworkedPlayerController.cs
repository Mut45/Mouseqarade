using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(PlayerMovement))]
public class NetworkedPlayerController : NetworkBehaviour
{
    [Header("Player Movement Reference")]
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private MouseAbilityController mouseAbility;

    //private PlayerInputNetworkData currentInputData;
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
    }

    public override void OnNetworkDespawn()
    {
        syncedInputData.OnValueChanged -= HandleSyncedInputChanged;
    }
    private void Update()
    {
        // if (!IsOwner) return;

        // PlayerInputNetworkData inputData = ReadLocalInput();
        // playerMovement.UpdateVisuals(inputData.InputDirection);
        // Debug.Log($"[CLIENT {OwnerClientId}] Input read: {inputData.InputDirection}");
        
        // SubmitInputDataViaServerRpc(inputData);
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

    private void HandleSyncedInputChanged(PlayerInputNetworkData prevInput, PlayerInputNetworkData curInput)
    {
        if (!IsServer) return;

        // Call handle input from 
        
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
        inputData.SecondaryPressed = Input.GetKeyDown(KeyCode.Q);
        inputData.InteractPressed = Input.GetKeyDown(KeyCode.E);

        return inputData; 
    }

    // [ServerRpc]
    // private void SubmitInputDataViaServerRpc(PlayerInputNetworkData inputData)
    // {
    //     if (inputData.InputDirection.sqrMagnitude > 1.0f) inputData.InputDirection = inputData.InputDirection.normalized;

    //     currentInputData = inputData;
    // }
    [ServerRpc]
    private void SubmitInputDataViaServerRpc(PlayerInputNetworkData inputData)
    {
        if (inputData.InputDirection.sqrMagnitude > 1.0f) 
            inputData.InputDirection = inputData.InputDirection.normalized;

        syncedInputData.Value = inputData;
    }
}