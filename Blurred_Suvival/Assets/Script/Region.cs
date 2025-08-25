using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{
    [Header("Enemy Pool for This Region")]
    public List<EnemyInZone> enemies;

    [Header("Spawn Manager / Enemy Handler")]
    public Enemy EnemyManager;

    public float EncounterChance = 0.15f;
    public int MaxEnemyCount = 4;

    public SquadMover squadMover;

    public void TrySpawnEnemies()
    {
        if (EnemyManager == null || enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("Missing EnemyManager or enemies not defined.");
            return;
        }

        EnemyManager.DestroyAllChildren();

        var validTiles = new List<(Transform tile, int row)>();
        var tiles = EnemyManager.tileManager.tiles;

        for (int row = 0; row < 4; row++)
        {
            for (int col = 13; col <= 15; col++)
            {
                var tile = tiles[row, col];
                if (tile == null) continue;

                var data = tile.GetComponent<TileData>();
                if (data != null && !data.IsOccupied)
                {
                    validTiles.Add((tile, row));
                }
            }
        }

        if (validTiles.Count == 0)
        {
            Debug.Log("No valid tiles to spawn enemies.");
            return;
        }

        validTiles.Shuffle();
        int ZombieLevel = ZombieLevelScaler.Instance.ScaleZombieLevel(squadMover.gameObject);

        foreach (var enemyInZone in enemies)
        {
            int spawnCount = Random.Range(enemyInZone.minCount, enemyInZone.maxCount + 1);

            for (int i = 0; i < spawnCount && validTiles.Count > 0; i++)
            {
                var (tile, row) = validTiles[0];
                validTiles.RemoveAt(0);

                var zombie = GetZombieFromPool(enemyInZone.ZombieName, ZombieLevel);

                if (zombie == null)
                {
                    Debug.LogWarning($"No available zombies in pool for: {enemyInZone.ZombieName}");
                    continue;
                }

                zombie.gameObject.SetActive(true);
                zombie.SetParent(EnemyManager.transform);

                int sortingOrder = 10 + row;
                Vector3 pos = tile.position;
                pos.z = -sortingOrder * 0.01f;
                zombie.position = pos;

                var sr = zombie.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = sortingOrder;

                var ai = zombie.GetComponent<ZombieAIBase>();
                if (ai != null) ai.Initialize(EnemyManager.tileManager, tile.GetComponent<TileData>());

                var stats = zombie.GetComponent<CharacterStats>();
                if (stats != null) stats.ScaleStatsByLevel();

                EnemyManager.spawnedEnemies.Add(zombie.gameObject);
            }
        }
    }

    private Transform GetZombieFromPool(string zombieName, int ZombieLevel)
    {
        foreach (Transform zombie in EnemyManager.ZombiePool)
        {
            var ai = zombie.GetComponent<ZombieAIBase>();
            if (ai != null && ai.ZombieName == zombieName)
            {
                zombie.GetComponent<CharacterStats>().Level = ZombieLevel;
                return zombie;
            }
        }
        return null;
    }

    public List<GameObject> battleGrounds;

    public void loadBattleGround()
    {
        if (battleGrounds == null || battleGrounds.Count == 0)
            return;
        int Rand = Random.Range(0, battleGrounds.Count);

        BattleGroundManager.Instance.LoadMap(battleGrounds[Rand]);
    }
}
