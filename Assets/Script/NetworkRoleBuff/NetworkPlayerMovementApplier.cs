using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerRoleState))]
[RequireComponent(typeof(PlayerDisguiseState))]
public class NetworkPlayerMovementApplier : MonoBehaviour
{
    [Header("Player State References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerRoleState roleState;
    [SerializeField] private PlayerDisguiseState disguiseState;
    [SerializeField] private NetworkRoleBuffState buffState;

    [Header("Player base speed")]
    [SerializeField] private float mouseBaseMoveSpeed = 5.0f;
    [SerializeField] private float catBaseMoveSpeed = 5.0f;

    [Header("Buff Bonuses")]
    [SerializeField] private float catPermanentMoveSpeedBonus = 1.0f;
    [SerializeField] private float catTemporaryMoveSpeedBonus = 2.0f;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (roleState == null) roleState = GetComponent<PlayerRoleState>();
        if (disguiseState == null) disguiseState = GetComponent<PlayerDisguiseState>();
    }

    private void OnEnable()
    {
        if (roleState != null)
        {
            roleState.OnRoleChanged += HandleRoleChanged;
        }

        if (disguiseState != null)
        {
            disguiseState.OnDisguiseChanged += HandleDisguiseChanged;
        }

        if (buffState != null)
        {
            buffState.OnCatBuffsChanged += UpdateMovementSpeed;
            buffState.OnMouseBuffsChanged += UpdateMovementSpeed;
            buffState.OnRuntimeStateChanged += UpdateMovementSpeed;
        }

    } 
    #region Movement speed
    private void HandleRoleChanged(PlayerRole role)
    {
        UpdateMovementSpeed();
    }

    private void UpdateMovementSpeed()
    {
        if (movement == null || roleState == null) return;

        float finalSpeed = GetBaseSpeedFromRoleAndDisguise();
        finalSpeed += GetBuffMoveSpeedBonus();

        movement.SetMovementSpeed(finalSpeed);
    }
    
    private float GetBaseSpeedFromRoleAndDisguise()
    {
        if (roleState.GetRole() == PlayerRole.Cat)
        {
            return catBaseMoveSpeed;
        }


        return 0f;
    }

    private float GetBuffMoveSpeedBonus()
    {
        return 0f;
    }
    #endregion

    private void HandleDisguiseChanged(bool isDisguised, ulong NOId)
    {
        
    }

}