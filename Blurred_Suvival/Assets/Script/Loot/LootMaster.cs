using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LootMaster", menuName = "Loot/LootMaster")]
public class LootMaster : ScriptableObject
{
    [Header("Loot by Rarity")]
    public List<LootEntry> commonLoot = new List<LootEntry>();
    public List<LootEntry> rareLoot = new List<LootEntry>();
    public List<LootEntry> epicLoot = new List<LootEntry>();
    public List<LootEntry> legendaryLoot = new List<LootEntry>();

    /// <summary>
    /// Get a random loot prefab from a specified rarity list based on drop chance
    /// </summary>
    public LootEntry GetRandomLoot(LootRarity rarity)
    {
        List<LootEntry> list = rarity switch
        {
            LootRarity.Common => commonLoot,
            LootRarity.Rare => rareLoot,
            LootRarity.Epic => epicLoot,
            LootRarity.Legendary => legendaryLoot,
            _ => commonLoot
        };

        if (list.Count == 0) return null;

        foreach (var entry in list)
        {
            if (Random.value <= entry.dropChance)
                return entry; // Found a valid drop
        }

        return null; // Nothing dropped this roll
    }

}

public enum LootRarity { Common, Rare, Epic, Legendary }
