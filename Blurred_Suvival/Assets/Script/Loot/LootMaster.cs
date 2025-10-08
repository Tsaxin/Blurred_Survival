using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

[CreateAssetMenu(fileName = "LootMaster", menuName = "Loot/LootMaster")]
public class LootMaster : ScriptableObject
{
    [Header("Loot by Rarity")]
    public List<LootEntry> commonLoot = new List<LootEntry>();
    public List<LootEntry> rareLoot = new List<LootEntry>();
    public List<LootEntry> epicLoot = new List<LootEntry>();
    public List<LootEntry> legendaryLoot = new List<LootEntry>();

    [Header("Rarity Weights")]
    [Range(0f, 1f)] public float commonWeight = 0.6f;    // 60% chance
    [Range(0f, 1f)] public float rareWeight = 0.25f;     // 25% chance
    [Range(0f, 1f)] public float epicWeight = 0.1f;      // 10% chance
    [Range(0f, 1f)] public float legendaryWeight = 0.05f; // 5% chance

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

    public List<LootEntry> GetAllCraftableLoot()
    {
        List<LootEntry> lootEntries = new List<LootEntry>();
        foreach (LootEntry lootEntry in commonLoot) {
            if (lootEntry.loot.GetComponent<CraftableItem>() != null)
            {
                lootEntries.Add(lootEntry);
            }
        }
        foreach (LootEntry lootEntry in rareLoot) {
            if (lootEntry.loot.GetComponent<CraftableItem>() != null)
            {
                lootEntries.Add(lootEntry);
            }
        }
        foreach (LootEntry lootEntry in epicLoot) {
            if (lootEntry.loot.GetComponent<CraftableItem>() != null)
            {
                lootEntries.Add(lootEntry);
            }
        }
        foreach (LootEntry lootEntry in legendaryLoot) {
            if (lootEntry.loot.GetComponent<CraftableItem>() != null)
            {
                lootEntries.Add(lootEntry);
            }
        }

        return lootEntries;
    }

    /// <summary>
    /// Pick a random rarity based on configured weights
    /// </summary>
    public LootRarity GetRandomRarity()
    {
        float roll = Random.value; // 0 to 1
        float cumulative = 0f;

        cumulative += commonWeight;
        if (roll < cumulative) return LootRarity.Common;

        cumulative += rareWeight;
        if (roll < cumulative) return LootRarity.Rare;

        cumulative += epicWeight;
        if (roll < cumulative) return LootRarity.Epic;

        return LootRarity.Legendary; // default if all else fails
    }

    public LootEntry GetLootByName(string lootName)
    {
        // Combine all loot lists into one search pool
        List<LootEntry> allLoot = new List<LootEntry>();
        allLoot.AddRange(commonLoot);
        allLoot.AddRange(rareLoot);
        allLoot.AddRange(epicLoot);
        allLoot.AddRange(legendaryLoot);

        // Find the loot with the matching name
        foreach (var entry in allLoot)
        {
            if (entry != null && entry.loot.GetComponent<ItemPickUp>().itemData.itemName == lootName)
                return entry;
        }

        // If no match found
        //Debug.LogWarning($"Loot with name {lootName} not found!");
        return null;
    }
}

[System.Serializable]
public class LootEntry
{
    public GameObject loot;
    [Range(0f, 1f)]
    public float dropChance; // e.g., 0.2 = 20% chance
}

public enum LootRarity { Common, Rare, Epic, Legendary }
