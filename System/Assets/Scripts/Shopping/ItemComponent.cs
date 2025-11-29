using UnityEngine;

public class ItemComponent : MonoBehaviour
{
    public ItemData data;
    public int GetPrice() => data.price;
    public ItemType GetItemType() => data.itemType;
}
