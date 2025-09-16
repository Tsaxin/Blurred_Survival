using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLootDropper : MonoBehaviour
{
    public void DropLoot(TileData tileData,int Level)
    {
        int RandomLootCount = Random.Range(0,Level);

        LootMasterManager.Instance.DropLootWithLevel(tileData,RandomLootCount,Level);
    }
}
