using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class NormalZombieAI : ZombieAIBase
{
    public CharacterStats characterStats;

    public override IEnumerator TakeTurn()
    {
        Debug.Log($"{name} is thinking...");

        CharacterStats targetPlayer = FindAdjacentPlayer();
        if (targetPlayer != null)
        {
            yield return StartCoroutine(AttackPlayer(targetPlayer));
        }
        else
        {
            CharacterStats attackTarget;
            TileData moveTile = FindChasingTileAndAttackOpportunity(out attackTarget);

            if (moveTile != null)
            {
                yield return MoveToTile(moveTile);

                // Check again after move
                if (attackTarget != null && !attackTarget.IsDead)
                {
                    Vector2Int playerPos = GetTileIndices(attackTarget.GetComponent<CharacterController>().currentTileData.transform);
                    Vector2Int currentPos = GetTileIndices(currentTileData.transform);
                    int distToPlayer = Mathf.Max(Mathf.Abs(currentPos.x - playerPos.x), Mathf.Abs(currentPos.y - playerPos.y));

                    if (distToPlayer <= 1)
                    {
                        Debug.LogWarning($"{name} attacks after moving to tile within range.");
                        yield return StartCoroutine(AttackPlayer(attackTarget));
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

    #region Animation Driven Attacks
    private CharacterStats _targetStats;
    private CharacterStats _myStats;
    private int _remainingAttacks;
    private bool _swingFinished;
    private bool _isAttacking;

    // =========================
    // Coroutine that controls the whole multi-attack
    // =========================
    IEnumerator AttackPlayer(CharacterStats target)
    {
        // prevent re-entrancy
        if (_isAttacking) yield break;
        _isAttacking = true;

        _myStats = GetComponent<CharacterStats>();
        if (_myStats == null || _myStats.IsDead || target == null)
        {
            _isAttacking = false;
            yield break;
        }

        _targetStats = target;
        _remainingAttacks = Mathf.Max(1, _myStats.AttackCount);

        // Face the player before starting
        TileData targetTile = target.GetComponent<CharacterController>()?.currentTileData;
        if (targetTile != null)
            ScaleCharacter(targetTile);

        // Attack loop
        while (_remainingAttacks > 0)
        {
            // Target died mid-chain?
            if (_targetStats == null || _targetStats.IsDead) break;

            // Trigger one swing
            PlayAttackAnimation();

            // Wait for animation end event
            _swingFinished = false;
            yield return new WaitUntil(() => _swingFinished);

            _remainingAttacks--;
        }

        // small cushion and release
        yield return new WaitForSeconds(0.05f);
        _isAttacking = false;
    }

    // =========================
    // Animation events
    // =========================

    // Called by animation event on the *hit* frame
    public void DealAttackDamage()
    {
        if (_targetStats == null || _targetStats.IsDead || _myStats == null) return;

        int damage = _myStats.attack;
        _targetStats.TakeDamage(damage, _myStats);
        Debug.Log($"{name} dealt {damage} damage.");
    }

    // Called by animation event at the *end* of the attack animation.
    // Keep your existing event name:
    public void CheckNextAttack()
    {
        // Do NOT start another animation here.
        // Just signal the coroutine to continue.
        _swingFinished = true;
    }

    // =========================
    // Helpers
    // =========================
    private void PlayAttackAnimation()
    {
        Animator anim = GetComponent<Animator>();
        anim.SetTrigger("Melee");
    }


    #endregion
}
