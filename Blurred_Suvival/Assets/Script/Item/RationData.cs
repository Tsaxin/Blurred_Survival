using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Items/Ration")]
public class RationData : ItemData
{
    public int RationRestoreAmount;

    private void OnEnable()
    {
        itemType = ItemType.Ration;
    }
}