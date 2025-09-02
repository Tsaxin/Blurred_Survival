using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLootGenerator : MonoBehaviour
{
    public LootMaster lootMaster;

    public List<LootGenerator> lootGenerators;

    public int MinLootColumn;

    public void LoadLootInTiles()
    {
        TileManager tileManager = GameObject.FindWithTag("Tile Manager").GetComponent<TileManager>();

        foreach (LootGenerator loot in lootGenerators)
        {
            for (int i = 0; i < loot.LootCount; i++)
            {
                LootEntry lootEntry = lootMaster.GetRandomLoot(loot.lootRarity);
                GameObject tile = tileManager.GetRandomTile().gameObject;

                GameObject obj = Instantiate(lootEntry.loot, tile.transform.position, Quaternion.identity);

                tile.GetComponent<TileData>().PlaceLoot(obj);
            }
        }
    }
}

[System.Serializable]
public class LootGenerator
{
    public LootRarity lootRarity;
    public int LootCount;
}
