using UnityEngine;

public class ZombieLootDropper : MonoBehaviour
{
    [Header("Loot Table")]
    public LootEntry[] lootTable;

    public void DropLoot(Vector3 position, TileData tile)
    {
        foreach (LootEntry entry in lootTable)
        {
            float roll = Random.value; // 0 to 1
            if (roll <= entry.dropChance)
            {
                Debug.Log($"Dropping loot: {entry.loot.GetComponent<ItemPickUp>().itemData.itemName}");

                GameObject loot = Instantiate(entry.loot, position, Quaternion.identity);

                // ----------------- ✅ New Code to assign to TileData -----------------
                if (tile != null)
                {
                    tile.PlaceLoot(loot);
                }

                //return; // Only one item drop max; remove this line if you want multiple drops
            }
        }
        //Debug.Log("No loot dropped.");
    }
}


[System.Serializable]
public class LootEntry
{
    public GameObject loot;
    [Range(0f, 1f)]
    public float dropChance; // e.g., 0.2 = 20% chance
}

