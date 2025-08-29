using Unity.VisualScripting;
using UnityEngine;

public class LootMasterManager : MonoBehaviour
{
    public LootMaster lootMaster;

    public static LootMasterManager Instance;

    void OnEnable()
    {
        if (Instance == null) Instance = this;
    }
    public void DropRandomLoot(LootRarity rarity, TileData tile)
    {
        LootEntry lootEntry = lootMaster.GetRandomLoot(rarity);
        Debug.Log(lootEntry);
        
        if (tile == null || lootMaster == null || lootEntry == null) return;

        float SpawnChance = lootEntry.dropChance;
        if (Random.value<SpawnChance)
        {
            GameObject lootPrefab = lootEntry.loot;
            if (lootPrefab == null) return;

            // Instantiate the loot at the tile's position
            GameObject loot = Instantiate(lootPrefab, tile.transform.position, Quaternion.identity);

            // Assign the loot to the tile
            tile.PlaceLoot(loot);
        }
    }

    public void DropRandomLoots(TileData tile)
    {
        if (tile == null || lootMaster == null) return;

        // Define how many times to roll per rarity
        int commonRolls = 2;
        int rareRolls = 1;
        int epicRolls = 1;
        int legendaryRolls = 1;

        // Drop Common loot
        for (int i = 0; i < commonRolls; i++)
            DropRandomLoot(LootRarity.Common, tile);

        // Drop Rare loot
        for (int i = 0; i < rareRolls; i++)
            DropRandomLoot(LootRarity.Rare, tile);

        // Drop Epic loot
        for (int i = 0; i < epicRolls; i++)
            DropRandomLoot(LootRarity.Epic, tile);

        // Drop Legendary loot
        for (int i = 0; i < legendaryRolls; i++)
            DropRandomLoot(LootRarity.Legendary, tile);
    }


}
