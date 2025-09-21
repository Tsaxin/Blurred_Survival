using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Region : MonoBehaviour
{
    [Header("Enemy Pool for This Region")]
    public List<EnemyInZone> enemies;

    [Header("Spawn Manager / Enemy Handler")]
    public Enemy EnemyManager;

    public float EncounterChance = 0.15f;
    public int EnemyStartColumn = 3, EnemyEndColumn = 15;
    public SquadMover squadMover;

    private void SpawnEnemiesInternal(
    TurnManager turnManager,
    int startCol,
    bool isPreemptive,
    Event eventData,
    System.Func<(Transform tile, int row), Transform> getEnemy)
    {
        var validTiles = new List<(Transform tile, int row)>();
        var tiles = EnemyManager.tileManager.tiles;

        for (int row = 0; row < 4; row++)
        {
            for (int col = startCol; col <= EnemyEndColumn; col++)
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
            if (zombie.GetComponent<CharacterController>() != null)
            {
                CharacterController CC = zombie.GetComponent<CharacterController>();
                CC.GetComponent<Tile>().CurrentTileData = tile.GetComponent<TileData>();
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

            // Convert GameObject list to ZombieAIBase list
            var zombieList = EnemyManager.spawnedEnemies
                .Select(go => go.GetComponent<ZombieAIBase>())
                .Where(z => z != null) // filter out anything that doesn’t have ZombieAIBase
                .ToList();
            ZombieSoundManager.Instance.StartZombieSound(zombieList); //Add sounds
        }
    }

    public void TrySpawnEnemies(TurnManager turnManager)
    {
        if (EnemyManager == null)
        {
            Debug.LogWarning("Missing EnemyManager.");
            return;
        }

        EnemyManager.DestroyAllChildren();

        foreach (var enemyInZone in enemies)
        {
            int spawnCount = Random.Range(enemyInZone.minCount, enemyInZone.maxCount + 1);

            SpawnEnemiesInternal(
                turnManager,
                startCol: EnemyStartColumn,
                isPreemptive: false,
                null,
                getEnemy: (tileRow) =>
                {
                    if (spawnCount-- <= 0) return null;
                    return GetZombieFromPool(enemyInZone.ZombieName);
                });
        }
    }

    public void TrySpawnAmbushEnemies(TurnManager turnManager)
    {
        if (EnemyManager == null)
        {
            Debug.LogWarning("Missing EnemyManager.");
            return;
        }

        EnemyManager.DestroyAllChildren();

        foreach (var enemyInZone in enemies)
        {
            int spawnCount = Random.Range(enemyInZone.minCount, enemyInZone.maxCount + 1);

            SpawnEnemiesInternal(
                turnManager,
                startCol: EnemyStartColumn,
                isPreemptive: false,
                null,
                getEnemy: (tileRow) =>
                {
                    if (spawnCount-- <= 0) return null;
                    return GetZombieFromPool(enemyInZone.ZombieName);
                });
        }
    }

    public void TrySpawnPreemptiveEnemies(TurnManager turnManager)
    {
        if (EnemyManager == null)
        {
            Debug.LogWarning("Missing EnemyManager.");
            return;
        }

        EnemyManager.DestroyAllChildren();

        foreach (var enemyInZone in enemies)
        {
            int spawnCount = Random.Range(enemyInZone.minCount, enemyInZone.maxCount + 1);

            SpawnEnemiesInternal(
                turnManager,
                startCol: EnemyStartColumn,
                isPreemptive: true,
                null,
                getEnemy: (tileRow) =>
                {
                    if (spawnCount-- <= 0) return null;
                    return GetZombieFromPool(enemyInZone.ZombieName);
                });
        }
    }

    public void TrySpawnEventTriggerEnemies(TurnManager turnManager, List<GameObject> objects,Event eventData)
    {
        if (EnemyManager == null)
        {
            Debug.LogWarning("Missing EnemyManager.");
            return;
        }

        EnemyManager.DestroyAllChildren();

        int index = 0;

        SpawnEnemiesInternal(
            turnManager,
            startCol: EnemyStartColumn,
            isPreemptive: false,
            eventData,
            getEnemy: (tileRow) =>
            {
                if (index >= objects.Count) return null;

                // Instantiate prefab at runtime
                GameObject instance = GameObject.Instantiate(objects[index++]);
                instance.GetComponent<RandomStat>().GenerateRandomStat(eventData.NPCLevel);

                instance.GetComponent<TurnIndicator>()?.SetIndicator(false);
                instance.GetComponent<CharacterController>()?.SetScale(-1f); //face left side

                return instance.transform;
            });
    }
    private Transform GetZombieFromPool(string zombieName)
    {
        foreach (Transform zombie in EnemyManager.ZombiePool)
        {
            if (!zombie.gameObject.activeInHierarchy) // ✅ only grab unused ones
            {
                var stats = zombie.GetComponent<CharacterStats>();
                if (stats != null && stats.CharacterName == zombieName)
                {
                    stats.Level = GetComponent<ZombieLevelScaler>().GetZombieLevel();
                    return zombie;
                }
            }
        }
        return null;
    }


    public List<GameObject> battleGrounds;

    public void loadBattleGround(GameObject BattleGround)
    {
        if (battleGrounds == null || battleGrounds.Count == 0)
            return;
        if (BattleGround == null)
        {
            int Rand = Random.Range(0, battleGrounds.Count);

            BattleGroundManager.Instance.LoadMap(battleGrounds[Rand]);
        }
        else
        {
            BattleGroundManager.Instance.LoadMap(BattleGround);
        }
    }
}
