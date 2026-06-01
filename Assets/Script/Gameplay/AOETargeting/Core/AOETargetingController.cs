using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AOE targeting system flow - Local only
/// </summary>

public class AOETargetingController : MonoBehaviour
{
    [SerializeField] private float indicatorMoveSpeed = 1f;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private NetworkedPlayerController playerController;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private ItemUseController itemUseController;
    private bool isTargeting = false;
    public bool IsTargeting => isTargeting;
    private GameObject activeIndicator;
    private Vector2 targetPosition;

    private ItemDefinition currentItemDefinition;
    private ItemUpgradeData currentLevelData;
    private readonly List<GameObject> highlightedTargets = new();

    private void Update()
    {
        if (!isTargeting) return;

        MoveIndicator();
        UpdateHighlights();

    }
    
    #region Targeting System Flow
    public void StartTargeting(ItemDefinition item, int itemLevel)
    {
        if (isTargeting) return;

        if (item.UseMode != ItemUseMode.AOETargeted) return;

        ItemUpgradeData levelData = item.GetLevelData(itemLevel);
        currentItemDefinition = item;
        currentLevelData = levelData;
        isTargeting = true;
        targetPosition = transform.position;

        // Lock player's movement
        LockPlayerMovement(true);
        // Instantiate the indicator prefab
        SpawnIndicator();
        // Update the highlighted object/npcs
        UpdateHighlights();

    }

    public void CancelTargeting()
    {
        if (!isTargeting) return;

        isTargeting = false;
        LockPlayerMovement(false);
        DestroyIndicator();
    }

    public void ConfirmTargeting(ItemDefinition item)
    {
        if (!isTargeting) return;
        
        if (currentItemDefinition == null || currentLevelData == null)
        {
            CancelTargeting();
            return;            
        }

        ItemUseNetworkRequestData request = new ItemUseNetworkRequestData
        {
            ItemId = currentItemDefinition.Id,
            HasTargetPosition = true,
            TargetPosition = targetPosition,

        };

        CancelTargeting();
        if (itemUseController != null)
        {
            itemUseController.RequestUseItem(request); // To server-authoratitive
        }
    }
    #endregion

    #region Spawning & Destorying Indicator
    private void SpawnIndicator()
    {
        if (indicatorPrefab == null) return;
        
        // Spawn local only visuals
        activeIndicator = Instantiate(indicatorPrefab, targetPosition, Quaternion.identity);
        activeIndicator.transform.localScale = new Vector3(currentLevelData.Radius * 2, currentLevelData.Radius * 2, currentLevelData.Radius * 2);

    }
    private void DestroyIndicator()
    {
        if (activeIndicator == null) return;

        Destroy(activeIndicator);
        activeIndicator = null;
    }

    #endregion

    #region AOE Targeting Visual Highlight
    private void UpdateHighlights()
    {
        if (currentLevelData == null) return;
        // TODO: Highlight affected objects/npcs
    }

    private void ClearHighlights()
    {
        // TODO: Clear the highlight on the affected objects/npcs
    }
    #endregion

    #region Indicator Movement
    private void MoveIndicator()
    {
        if (currentLevelData == null) return;

        Vector2 input = ReadIndicatorMoveInput();
        targetPosition += input * indicatorMoveSpeed * Time.deltaTime;
        ClampTargetPositionToMaxDistance();
        if (activeIndicator != null)
        {
            activeIndicator.transform.position = targetPosition;
        }
    }
    #endregion

    #region Helpers
    private void LockPlayerMovement(bool locked)
    {
        // Lock local player movement too to prevent queueing up unnecessary inputs commands
        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(locked);
        }

        if (playerController == null)
        {
            playerController.RequestMovementLock(locked);
        }

    }

    private Vector2 ReadIndicatorMoveInput()
    {
        Vector2 input = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            input.y += 1f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            input.y -= 1f;
        }
        
        if (Input.GetKey(KeyCode.RightArrow))
        {
            input.x += 1f;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            input.x -= 1f;
        }

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }
        return input;
    }

    private void ClampTargetPositionToMaxDistance()
    {
        Vector2 playerPosition = transform.position;
        Vector2 offset = targetPosition - playerPosition;

        float maxDistance = currentLevelData.MaxUseDistance;

        if (offset.magnitude > maxDistance)
        {
            targetPosition = playerPosition + offset.normalized * maxDistance;
        }
    }
    #endregion
}