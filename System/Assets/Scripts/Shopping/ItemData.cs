using UnityEditor.Build.Content;
using UnityEngine;

public enum ItemType { Milk, Biscuit, Pencil }
public enum MilkType { Original, Chocolate, StrawberryYogurt }
public enum BiscuitType { Normal, Nuts }
public enum PencilType { Red, HB, Black }


[System.Serializable]
public class ItemData
{
    public ItemType itemType;

    public string itemName;
    public int price;

    public MilkType milkType;

    public BiscuitType biscuitType;

    public PencilType pencilType;
}
