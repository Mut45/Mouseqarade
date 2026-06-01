using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bridges the item stats data and runtime game logic effect and stores all of the available items
/// </summary>
public class ItemDatabase : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemDefinition[] itemDefinitions;
    [Header("Runtime Item Effects")]
    [SerializeField] private MonoBehaviour[] itemEffectBehaviours;

    private Dictionary<ItemId, ItemDefinition> itemDefinitionsById = new();
    private Dictionary<ItemId, IUsableItem> itemRuntimeEffectsById = new();
    private void Awake()
    {
        BuildDefinitionLookup();
        BuildRuntimeEffectsLookup();
    }

    private void BuildDefinitionLookup()
    {
        itemDefinitionsById.Clear();

        foreach (ItemDefinition definition in itemDefinitions)
        {
            if (definition == null)
                continue;

            if (definition.Id == ItemId.None)
            {
                Debug.LogWarning($"Item definition {definition.name} has ItemId.None.");
                continue;
            }

            itemDefinitionsById[definition.Id] = definition;
        }
    }

    private void BuildRuntimeEffectsLookup()
    {
        itemRuntimeEffectsById.Clear();

        foreach (IUsableItem itemEffect in itemEffectBehaviours)
        {
            if (itemEffect == null)
            {
                continue;
            }

            if (itemEffect.ItemId == ItemId.None)
            {
                continue;
            }

            itemRuntimeEffectsById[itemEffect.ItemId] = itemEffect;            
        }
    }

    public bool TryGetDefinition(ItemId id, out ItemDefinition definition)
    {
        return itemDefinitionsById.TryGetValue(id, out definition);
    }

    public bool TryGetEffect(ItemId id, out IUsableItem effect)
    {
        return itemRuntimeEffectsById.TryGetValue(id, out effect);
    }

    public bool TryGetDefinitionAndEffectById(ItemId id, out ItemDefinition definition, out IUsableItem effect)
    {
        bool hasDefinition = TryGetDefinition(id, out definition);
        bool hasEffect = TryGetEffect(id, out effect);
        
        return hasDefinition && hasEffect;
    }
    
}