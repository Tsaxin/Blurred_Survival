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
                //Debug.Log($"Dropping loot: {entry.loot.GetComponent<ItemPickUp>().itemData.itemName}");

                GameObject loot = Instantiate(entry.loot, position, Quaternion.identity);

                // ----------------- ✅ New Code to assign to TileData -----------------
                if (tile != null)
                {
                    tile.PlaceLoot(loot);
                }
            }
        }
    }
}

