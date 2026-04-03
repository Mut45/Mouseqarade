using Unity.Netcode;
using Unity.VisualScripting;
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

    private void OnDisable()
    {
        if (roleState != null)
        {
            roleState.OnRoleChanged -= HandleRoleChanged;
        }

        if (disguiseState != null)
        {
            disguiseState.OnDisguiseChanged -= HandleDisguiseChanged;
        }

        if (buffState != null)
        {
            buffState.OnCatBuffsChanged -= UpdateMovementSpeed;
            buffState.OnMouseBuffsChanged -= UpdateMovementSpeed;
            buffState.OnRuntimeStateChanged -= UpdateMovementSpeed;
        }
    }
    private void Start()
    {
        UpdateMovementSpeed();
    }
    #region Movement speed
    private void HandleRoleChanged(PlayerRole role)
    {
        UpdateMovementSpeed();
    }

    private void HandleDisguiseChanged(bool isDisguised, ulong NOId)
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

        float result = mouseBaseMoveSpeed;
        
        if (disguiseState == null || !disguiseState.GetIsDisguised())
        {
            return result;
        }

        // Mouse disguised
        ulong npcNOId = disguiseState.GetTargetedNPCNOId();

        if (!NetworkLookUp.TryGetNetworkNpc(NetworkManager.Singleton, npcNOId, out NetworkNPCController npc) || npc == null)
        {
            return result;
        }

        NPCRole npcRole = npc.GetRole();
        if (npcRole == null)
        {
            return result;
        }

        return npcRole.moveSpeed;
    }

    private float GetBuffMoveSpeedBonus()
    {
        if (buffState == null || roleState == null) return 0f;
        if (roleState.GetRole() != PlayerRole.Cat) return 0f;

        float bonus = 0f;

        if (buffState.HasBuffForRole(PlayerRole.Cat, BuffCardEffectId.CatMoveSpeed_Permanent))
        {
            bonus += catPermanentMoveSpeedBonus;
        }

        if (buffState.HasBuffForRole(PlayerRole.Cat, BuffCardEffectId.CatMoveSpeed_OnFail_Temporary)
            && buffState.IsCatTempSpeedActive)
        {
            bonus += catTemporaryMoveSpeedBonus;
        }

        return bonus;
    }
    #endregion



}