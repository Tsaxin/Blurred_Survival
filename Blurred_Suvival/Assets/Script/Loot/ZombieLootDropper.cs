using UnityEngine;

public class ZombieLootDropper : MonoBehaviour
{
    [Header("Loot Table")]
    public LootEntry[] lootTable;

    [Header("Loot Settings")]
    public int lootCount = 1; // number of loot items to drop per zombie

    public void DropLoot(Vector3 position, TileData tile)
    {
        // Calculate total weight
        float totalWeight = 0f;
        foreach (var entry in lootTable)
            totalWeight += entry.dropChance;

        // Total weight must include "no loot" if less than 100%
        float maxWeight = Mathf.Max(totalWeight, 1f); // ensures we have chance for no loot
        float noLootWeight = 1f - totalWeight; // portion of weight representing no loot
        noLootWeight = Mathf.Max(noLootWeight, 0f); // clamp to 0 if totalWeight >= 1

        for (int i = 0; i < lootCount; i++)
        {
            // Pick random number between 0 and total weight + noLootWeight
            float roll = Random.value * (totalWeight + noLootWeight);
            float cumulative = 0f;

            bool dropped = false;

            // Check each loot entry
            foreach (var entry in lootTable)
            {
                cumulative += entry.dropChance;
                if (roll <= cumulative)
                {
                    GameObject loot = Instantiate(entry.loot, position, Quaternion.identity);
                    if (tile != null)
                        tile.PlaceLoot(loot);

                    dropped = true;
                    break; // stop at first match
                }
            }

            // If nothing matched, it's "no loot"
            if (!dropped)
            {
                // Optional: Debug.Log("No loot dropped this roll.");
            }
        }
    }
}