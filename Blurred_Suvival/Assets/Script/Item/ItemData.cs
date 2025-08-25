using UnityEngine;

public enum ItemType
{
    Weapon,
    Consumable,
    // Add more types if needed later (e.g. Armor, KeyItems, etc.)
}

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public Sprite itemIcon;

    public int MaxQuantity=20;
}

[System.Serializable]
public class ItemInstance
{
    public ItemData data;
    public int durability; // For weapons or items that can degrade
    public int quantity;   // For stackable items like bandages or med packs

    public ItemInstance(ItemData data, int quantity = 1)
    {
        this.data = data;
        this.quantity = quantity;

        if (data is WeaponData)
            durability = 100; // or appropriate default
        else
            durability = -1; // no durability for consumables by default
    }
}
