using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLootItemGenerator : MonoBehaviour
{
    public LootMaster lootMaster;

    public List<string> LootName;

    public int MinLootColumn;
    public bool HasBeenLooted = false;

    public TileManager tileManager;
    public Squad squad;

    void OnEnable()
    {
        if (!HasBeenLooted)
        {
            squad.EventLootGeneratorByName=LoadLootInTiles;
            HasBeenLooted = true;
        }
    }

    public void LoadLootInTiles()
    {
        foreach (string lootName in LootName)
        {
            LootEntry lootEntry = lootMaster.GetLootByName(lootName);

            Transform tileTransform = tileManager.GetRandomTile(MinLootColumn);
            if (tileTransform == null)
            {
                Debug.LogWarning($"No valid tile found for {lootName}");
                continue; // skip this loot and move on
            }

            GameObject tile = tileTransform.gameObject;

            GameObject obj = Instantiate(lootEntry.loot, tile.transform.position, Quaternion.identity);

            tile.GetComponent<TileData>().PlaceLoot(obj);
            squad.EventLootGeneratorByName = null;
        }
    }
}
