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

    private void SpawnEnemiesInternal(
    TurnManager turnManager,
    int startCol,
    bool isPreemptive,
    System.Func<(Transform tile, int row), Transform> getEnemy)
    {
        if (EnemyManager == null)
        {
            Debug.LogWarning("Missing EnemyManager.");
            return;
        }

        EnemyManager.DestroyAllChildren();

        var validTiles = new List<(Transform tile, int row)>();
        var tiles = EnemyManager.tileManager.tiles;

        for (int row = 0; row < 4; row++)
        {
            for (int col = startCol; col <= 15; col++)
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

        // Keep spawning while we have tiles
        while (validTiles.Count > 0)
        {
            var (tile, row) = validTiles[0];
            validTiles.RemoveAt(0);

            var zombie = getEnemy((tile, row));
            if (zombie == null) continue;

            if (isPreemptive)
            {
                var aiPre = zombie.GetComponent<ZombieAIBase>();
                if (aiPre != null) aiPre.ScaleCharacter(-1);
            }

            zombie.gameObject.SetActive(true);
            zombie.SetParent(EnemyManager.transform);

            // Place zombie at tile position
            zombie.position = tile.position;

            //Also if has playercontroller then
            if (zombie.GetComponent<CharacterController>()!=null) {
                CharacterController CC = zombie.GetComponent<CharacterController>();
                CC.currentTileData = tile.GetComponent<TileData>();
                CC.turnManager = turnManager;
            }

            // Apply sorting order from ZombieAIBase
            var ai = zombie.GetComponent<ZombieAIBase>();
            if (ai != null)
            {
                ai.Initialize(EnemyManager.tileManager, tile.GetComponent<TileData>());
                ai.SortingOrder(tile.gameObject);
            }

            var stats = zombie.GetComponent<CharacterStats>();
            if (stats != null) stats.ScaleStatsByLevel();

            EnemyManager.spawnedEnemies.Add(zombie.gameObject);
        }
    }

    public void TrySpawnEnemies(TurnManager turnManager)
    {
        int ZombieLevel = ZombieLevelScaler.Instance.ScaleZombieLevel(squadMover.gameObject);

        foreach (var enemyInZone in enemies)
        {
            int spawnCount = Random.Range(enemyInZone.minCount, enemyInZone.maxCount + 1);

            SpawnEnemiesInternal(
                turnManager,
                startCol: 4,
                isPreemptive: false,
                getEnemy: (tileRow) =>
                {
                    if (spawnCount-- <= 0) return null;
                    return GetZombieFromPool(enemyInZone.ZombieName, ZombieLevel);
                });
        }
    }

    public void TrySpawnAmbushEnemies(TurnManager turnManager)
    {
        int ZombieLevel = ZombieLevelScaler.Instance.ScaleZombieLevel(squadMover.gameObject);

        foreach (var enemyInZone in enemies)
        {
            int spawnCount = Random.Range(enemyInZone.minCount, enemyInZone.maxCount + 1);

            SpawnEnemiesInternal(
                turnManager,
                startCol: 3,
                isPreemptive: false,
                getEnemy: (tileRow) =>
                {
                    if (spawnCount-- <= 0) return null;
                    return GetZombieFromPool(enemyInZone.ZombieName, ZombieLevel);
                });
        }
    }

    public void TrySpawnPreemptiveEnemies(TurnManager turnManager)
    {
        int ZombieLevel = ZombieLevelScaler.Instance.ScaleZombieLevel(squadMover.gameObject);

        foreach (var enemyInZone in enemies)
        {
            int spawnCount = Random.Range(enemyInZone.minCount, enemyInZone.maxCount + 1);

            SpawnEnemiesInternal(
                turnManager,
                startCol: 4,
                isPreemptive: true,
                getEnemy: (tileRow) =>
                {
                    if (spawnCount-- <= 0) return null;
                    return GetZombieFromPool(enemyInZone.ZombieName, ZombieLevel);
                });
        }
    }

    public void TrySpawnEventTriggerEnemies(TurnManager turnManager,List<GameObject> objects)
    {
        int index = 0;

        SpawnEnemiesInternal(
            turnManager,
            startCol: 4,
            isPreemptive: false,
            getEnemy: (tileRow) =>
            {
                if (index >= objects.Count) return null;

                // Instantiate prefab at runtime
                GameObject instance = GameObject.Instantiate(objects[index++]);

                instance.GetComponent<TurnIndicator>()?.SetIndicator(false);
                instance.GetComponent<CharacterController>()?.SetScale(-1f); //face left side

                return instance.transform;
            });
    }
    private Transform GetZombieFromPool(string zombieName, int ZombieLevel)
    {
        foreach (Transform zombie in EnemyManager.ZombiePool)
        {
            var ai = zombie.GetComponent<ZombieAIBase>();
            if (ai != null && ai.GetComponent<CharacterStats>().CharacterName == zombieName)
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
