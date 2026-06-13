using UnityEngine;

public class ShuffleCardItem : MonoBehaviour, IUsableItem
{
    private ItemId itemId;
    private ItemUseMode useMode;
    public ItemId ItemId => itemId;
    public ItemUseMode UseMode => useMode;

    public void UseServer(ItemUseContext context)
    {
        //TODO: Implement actual effects of using the Shuffle Card
        
    }
}