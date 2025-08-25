using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakerZombieAI : ZombieAIBase
{
    public CharacterStats characterStats;

    public override IEnumerator TakeTurn()
    {
        Debug.Log($"{name} (Shaker) is taking its turn...");

        bool hasAttacked = false;
        yield return null;

        CharacterStats targetPlayer = FindAdjacentPlayer();
        if (targetPlayer != null)
        {
            yield return ShakerAttack(targetPlayer);
            hasAttacked = true;
        }
        else
        {
            CharacterStats attackTarget;
            TileData moveTile = FindChasingTileAndAttackOpportunity(out attackTarget);

            if (moveTile != null)
            {
                Vector2Int oldPos = GetTileIndices(currentTileData.transform);
                yield return MoveToTile(moveTile);

                if (!hasAttacked && attackTarget != null && !attackTarget.IsDead)
                {
                    Vector2Int playerPos = GetTileIndices(attackTarget.GetComponent<CharacterController>().currentTileData.transform);
                    Vector2Int currentPos = GetTileIndices(currentTileData.transform);
                    int distToPlayer = Mathf.Max(Mathf.Abs(currentPos.x - playerPos.x), Mathf.Abs(currentPos.y - playerPos.y));

                    if (distToPlayer <= 1)
                    {
                        Debug.LogWarning($"{name} performs shaker attack after moving.");
                        yield return ShakerAttack(attackTarget);
                    }
                }
            }
            else
            {
                Debug.Log($"{name} has no move options and skips turn.");
            }
        }

        yield return null;
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
                TileData tile = tileManager.tiles[row, col]?.GetComponent<TileData>(); ;
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

        return null;
    }

    public TileData FindChasingTileAndAttackOpportunity(out CharacterStats attackTarget)
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

                TileData tile = tileManager.tiles[row, col]?.GetComponent<TileData>(); ;
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
                    attackTarget = nearestPlayer;
                    return tile;
                }

                if (distFromCurrent == 1)
                    allAdjacentBlocked = false;
            }
        }

        if (movableTiles.Count == 0 || allAdjacentBlocked)
        {
            Debug.Log($"{name} cannot move — all adjacent tiles blocked.");
            return null;
        }

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

    #region Animation attack
    private CharacterStats _targetStats;
    private int _remainingAttacks;
    private bool _isKnockingBack = false;

    IEnumerator ShakerAttack(CharacterStats mainTarget)
    {
        CharacterStats myStats = GetComponent<CharacterStats>();
        if (myStats == null || myStats.IsDead || mainTarget == null) yield break;

        _targetStats = mainTarget;
        _remainingAttacks = Mathf.Max(1, characterStats.AttackCount);

        // ✅ Get player's tile
        TileData targetTile = mainTarget.GetComponent<CharacterController>()?.currentTileData;
        if (targetTile != null)
        {
            // Call base function to face the player
            ScaleCharacter(targetTile);
        }

        //play animation
        PlayAttackAnimation();
        //hold the turn
        while (_remainingAttacks>0) {
            yield return null;
        }
    }

    // Called by animation event during hit frame
    public void DealAttackDamage()
    {
        int baseDamage = GetComponent<CharacterStats>().attack;
        List<CharacterStats> affectedTargets = new List<CharacterStats> { _targetStats };

        // Find all adjacent players around mainTarget
        Vector2Int center = GetTileIndices(_targetStats.GetComponent<CharacterController>().currentTileData.transform);
        int[,] dirs = new int[,]
        {
        { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 },
        { -1, -1 }, { 1, -1 }, { -1, 1 }, { 1, 1 }
        };

        for (int i = 0; i < dirs.GetLength(0); i++)
        {
            int dx = dirs[i, 0];
            int dy = dirs[i, 1];
            int row = center.y + dy;
            int col = center.x + dx;

            if (row >= 0 && row < tileManager.tiles.GetLength(0) &&
                col >= 0 && col < tileManager.tiles.GetLength(1))
            {
                TileData tile = tileManager.tiles[row, col]?.GetComponent<TileData>();
                if (tile != null && tile.IsOccupied && tile.occupant != null)
                {
                    CharacterStats stats = tile.occupant.GetComponent<CharacterStats>();
                    if (stats != null && !stats.IsDead && tile.occupant.CompareTag("Player"))
                    {
                        affectedTargets.Add(stats);
                    }
                }
            }
        }

        // Apply damage to all targets at once
        foreach (CharacterStats target in affectedTargets)
        {
            if (!target.IsDead)
            {
                Debug.Log($"{name} shaker attacks {target.name}");
                target.TakeDamage(baseDamage);
            }
        }
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
