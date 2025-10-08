using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftableItem : MonoBehaviour
{
    public List<CraftRequirement> requirements;

    public ItemData GetItemData()
    {
        return GetComponent<ItemPickUp>().itemData;
    }
}
