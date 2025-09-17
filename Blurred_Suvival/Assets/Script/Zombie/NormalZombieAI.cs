using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class NormalZombieAI : ZombieAIBase
{
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
