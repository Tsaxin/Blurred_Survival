using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exploder : ZombieAIBase
{
    [Tooltip("Number of adjacent units (players or zombies) needed to trigger explosion.")]
    public int ExplodeIfSurroundedBy = 3;

    public int explosionDamage = 20; // Damage dealt to each adjacent unit on explosion

    [Tooltip("How far the explosion should reach out from center tile.")]
    public int explosionRadius = 1;

    bool IsExploded;
    public override IEnumerator TakeTurn()
    {
        Debug.Log($"{name} is thinking...");

        if (CheckAndExplode())
        {
            IsExploded = false;
            PlayExplosionAnimation();

            // Wait for CharacterStats to die
            CharacterStats stats = GetComponent<CharacterStats>();
            while (!stats.IsDead)
            {
                yield return null;
            }

            // At this point, CharacterStats already destroyed the GameObject
            // Just end the coroutine safely
            yield break;
        }


        bool hasAttacked = false;

        CharacterStats targetPlayer = FindAdjacentPlayer();
        if (targetPlayer != null)
        {
            yield return AttackPlayer(targetPlayer);
            hasAttacked = true;
        }
        else
        {
            CharacterStats attackTarget;
            TileData moveTile = FindChasingTileAndAttackOpportunity(out attackTarget);

            if (moveTile != null)
            {
                yield return MoveToTile(moveTile);

                if (!hasAttacked && attackTarget != null && !attackTarget.IsDead)
                {
                    Vector2Int playerPos = GetTileIndices(attackTarget.GetComponent<CharacterController>().currentTileData.transform);
                    Vector2Int currentPos = GetTileIndices(currentTileData.transform);
                    int distToPlayer = Mathf.Max(Mathf.Abs(currentPos.x - playerPos.x), Mathf.Abs(currentPos.y - playerPos.y));

                    if (distToPlayer <= 1)
                    {
                        Debug.LogWarning($"{name} attacks after moving to tile within range.");
                        yield return AttackPlayer(attackTarget);
                        hasAttacked = true;
                    }
                }
            }
            else
            {
                Debug.Log($"{name} has no available move and skips the turn.");
            }
        }

        yield return new WaitForSeconds(0.1f); // Always yield before exiting
    }

    /// <summary>
    /// Checks if the Exploder is surrounded by at least ExplodeIfSurroundedBy units and triggers explosion.
    /// Returns true if exploded (turn ends), false otherwise.
    /// </summary>
    List<CharacterStats> targetsToDamage;
    bool CheckAndExplode()
    {
        Vector2Int index = GetTileIndices(currentTileData.transform);

        List<Transform> tilesInRange = tileManager.GetSurroundingTiles(index, explosionRadius);

        int nearbyTargetCount = 0;
        targetsToDamage = new List<CharacterStats>();

        foreach (Transform tileTransform in tilesInRange)
        {
            TileData tile = tileTransform.GetComponent<TileData>();
            if (tile != null && tile.IsOccupied && tile.occupant != null)
            {
                CharacterStats stats = tile.occupant.GetComponent<CharacterStats>();

                if (stats != null && !stats.IsDead && (tile.occupant.CompareTag("Player") || tile.occupant.CompareTag("Enemy")))
                {
                    if (tile.occupant.CompareTag("Player"))
                    {
                        nearbyTargetCount++;
                        targetsToDamage.Add(stats);
                    }
                    else if (tile.occupant.CompareTag("Enemy"))
                    {
                        targetsToDamage.Add(stats);
                    }
                }
            }
        }

        if (nearbyTargetCount >= ExplodeIfSurroundedBy)
        {
            return true;
        }

        return false;
    }

    CharacterStats FindAdjacentPlayer()
    {
        Vector2Int index = GetTileIndices(currentTileData.transform);
        int[,] directions = new int[,]
        {
            { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 },
            { -1, -1 }, { 1, -1 }, { -1, 1 }, { 1, 1 }
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int dx = directions[i, 0];
            int dy = directions[i, 1];

            int row = index.y + dy;
            int col = index.x + dx;

            if (row >= 0 && row < tileManager.tiles.GetLength(0) &&
                col >= 0 && col < tileManager.tiles.GetLength(1))
            {
                TileData tile = tileManager.tiles[row, col]?.GetComponent<TileData>();
                if (tile != null && tile.IsOccupied && tile.occupant != null)
                {
                    GameObject occupant = tile.occupant;
                    CharacterStats stats = occupant.GetComponent<CharacterStats>();

                    if (stats != null && !stats.IsDead && occupant.CompareTag("Player"))
                    {
                        Debug.Log($"{name} found adjacent player: {occupant.name}");
                        return stats;
                    }
                }
            }
        }

        Debug.Log($"{name} found no adjacent player.");
        return null;
    }

    #region Animation attack
    private CharacterStats _targetStats;
    private int _remainingAttacks;


    void PlayExplosionAnimation()
    {
        Animator anim = GetComponent<Animator>();
        anim.SetTrigger("Explode"); // assumes you already have "Melee" anim
    }
    public void Explode()
    {
        foreach (CharacterStats target in targetsToDamage)
        {
            Debug.Log($"{name} damages {target.name} for {explosionDamage} damage.");
            target.TakeDamage(explosionDamage);
        }

        if (characterStats != null)
        {
            Debug.Log($"{name} dies in explosion.");
            BloodPool.Instance.SpawnExplosion(this.transform);
            characterStats.Kill();
        }
        else
        {
            Debug.LogWarning($"{name} has no CharacterStats; destroying GameObject.");

            IsExploded = true;
        }
    }

    IEnumerator AttackPlayer(CharacterStats target)
    {
        CharacterStats myStats = GetComponent<CharacterStats>();
        if (myStats == null || myStats.IsDead || target == null) yield break;

        _targetStats = target;
        _remainingAttacks = Mathf.Max(1, characterStats.AttackCount);

        // ✅ Get player's tile
        TileData targetTile = target.GetComponent<CharacterController>()?.currentTileData;
        if (targetTile != null)
        {
            // Call base function to face the player
            ScaleCharacter(targetTile);
        }

        PlayAttackAnimation();

        while (_remainingAttacks > 0)
        {
            yield return null;
        }
    }

    // Called by animation event during hit frame
    public void DealAttackDamage()
    {
        if (_targetStats == null || _targetStats.IsDead) return;

        _targetStats.TakeDamage(GetComponent<CharacterStats>().attack);
    }

    // Called by animation event at end of animation
    public void CheckNextAttack()
    {
        _remainingAttacks--;

        if (_remainingAttacks > 0 && _targetStats != null && !_targetStats.IsDead)
        {
            Debug.Log($"{name} chaining attack with knockback. {_remainingAttacks} left.");
            PlayAttackAnimation();
        }
        else
        {
            Debug.Log($"{name} finished knocker attacks.");
            StartCoroutine(FinishTurnAfterDelay());
        }
    }

    private void PlayAttackAnimation()
    {
        Animator anim = GetComponent<Animator>();
        anim.SetTrigger("Melee"); // assumes you already have "Melee" anim
    }

    private IEnumerator FinishTurnAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        // tell turn manager you’re done (depends on your system)
    }

    #endregion

    TileData FindChasingTileAndAttackOpportunity(out CharacterStats attackTarget)
    {
        attackTarget = null;

        Vector2Int myIndex = GetTileIndices(currentTileData.transform);
        CharacterStats nearestPlayer = null;
        float minDistance = float.MaxValue;
        Vector2Int playerIndex = Vector2Int.zero;

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            CharacterStats stats = obj.GetComponent<CharacterStats>();
            if (stats != null && !stats.IsDead)
            {
                TileData playerTile = stats.GetComponent<CharacterController>()?.currentTileData;
                if (playerTile != null)
                {
                    Vector2Int pIndex = GetTileIndices(playerTile.transform);
                    float dist = Vector2Int.Distance(myIndex, pIndex);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nearestPlayer = stats;
                        playerIndex = pIndex;
                    }
                }
            }
        }

        if (nearestPlayer == null)
            return null;

        int moveRange = characterStats != null ? characterStats.MovementRange : 1;
        List<TileData> movableTiles = new List<TileData>();
        bool allAdjacentBlocked = true;

        for (int dy = -moveRange; dy <= moveRange; dy++)
        {
            for (int dx = -moveRange; dx <= moveRange; dx++)
            {
                int row = myIndex.y + dy;
                int col = myIndex.x + dx;

                if (row < 0 || row >= tileManager.tiles.GetLength(0) ||
                    col < 0 || col >= tileManager.tiles.GetLength(1))
                    continue;

                TileData tile = tileManager.tiles[row, col].GetComponent<TileData>();
                if (tile == null || tile.IsOccupied)
                    continue;

                Vector2Int candidatePos = new Vector2Int(col, row);
                int distFromCurrent = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                if (distFromCurrent > moveRange) continue;

                movableTiles.Add(tile);

                int distToPlayerX = Mathf.Abs(candidatePos.x - playerIndex.x);
                int distToPlayerY = Mathf.Abs(candidatePos.y - playerIndex.y);
                int chebyshevDistToPlayer = Mathf.Max(distToPlayerX, distToPlayerY);

                if (chebyshevDistToPlayer == 1 && distFromCurrent < moveRange)
                {
                    // Can move and attack from here
                    attackTarget = nearestPlayer;
                    return tile;
                }

                // If any adjacent tile is not blocked, flag false
                if (distFromCurrent == 1)
                    allAdjacentBlocked = false;
            }
        }

        if (movableTiles.Count == 0 || allAdjacentBlocked)
        {
            Debug.Log($"{name} cannot move — all adjacent tiles blocked.");
            return null;
        }

        // Move toward player even if no attack possible
        TileData bestTile = null;
        float bestDistance = float.MaxValue;

        foreach (TileData tile in movableTiles)
        {
            Vector2Int pos = GetTileIndices(tile.transform);
            float dist = Vector2Int.Distance(pos, playerIndex);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestTile = tile;
            }
        }

        return bestTile;
    }

    public override Vector2Int GetTileIndices(Transform tile)
    {
        for (int row = 0; row < tileManager.tiles.GetLength(0); row++)
        {
            for (int col = 0; col < tileManager.tiles.GetLength(1); col++)
            {
                if (tileManager.tiles[row, col] == tile)
                    return new Vector2Int(col, row);
            }
        }
        return new Vector2Int(-1, -1);
    }
}
