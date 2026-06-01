using UnityEngine;
public interface IUsableItem
{
    public ItemId ItemId {get;}  //property instead of instance variable since interface cannot keep instance variables
    public ItemUseMode UseMode{get;}  
    public void UseServer(ItemUseContext context);
}