using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Items/Consumable")]
public class ConsumableData : ItemData
{
    public int healthRestoreAmount;

    private void OnEnable()
    {
        itemType = ItemType.Consumable;
    }
}