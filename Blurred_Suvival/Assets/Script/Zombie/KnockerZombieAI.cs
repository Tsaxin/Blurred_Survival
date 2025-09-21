using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockerZombieAI : ZombieAIBase
{
    [Tooltip("How far to knock back the target (in tiles).")]
    public int knockbackRange = 2;

    [Tooltip("How fast the knockback effect moves the target (units per second).")]
    public float knockbackSpeed = 10f;

    public override IEnumerator TakeTurn()
    {
        Debug.Log($"{name} (Knocker) is thinking...");

        bool hasAttacked = false;

        CharacterStats adjacentTarget = FindAdjacentPlayer();
        if (adjacentTarget != null)
        {
            yield return AttackWithKnockback(adjacentTarget);
            hasAttacked = true;
        }
        else
        {
            CharacterStats chaseTarget;
            TileData tileToMove = FindChasingTileAndAttackOpportunity(out chaseTarget);

            if (tileToMove != null)
            {
                // ✅ Remember the starting tile before moving
                TileData previousTile = GetComponent<Tile>().CurrentTileData;

                yield return StartCoroutine(MoveToTile(tileToMove));

                // ✅ Calculate distance moved after reaching new tile
                if (previousTile != null && GetComponent<Tile>().CurrentTileData != null)
                {
                    Vector2Int prevPos = GetTileIndices(previousTile.transform);
                    Vector2Int newPos = GetTileIndices(GetComponent<Tile>().CurrentTileData.transform);

                    lastTilesMoved = Mathf.Max(
                        Mathf.Abs(prevPos.x - newPos.x),
                        Mathf.Abs(prevPos.y - newPos.y)
                    );
                }
                else
                {
                    lastTilesMoved = 0; // fallback
                }

                // ✅ Now check attack possibility
                if (!hasAttacked && chaseTarget != null && !chaseTarget.IsDead)
                {
                    Vector2Int playerPos = GetTileIndices(chaseTarget.GetComponent<Tile>().CurrentTileData.transform);
                    Vector2Int currentPos = GetTileIndices(GetComponent<Tile>().CurrentTileData.transform);
                    int distToPlayer = Mathf.Max(Mathf.Abs(currentPos.x - playerPos.x), Mathf.Abs(currentPos.y - playerPos.y));

                    if (distToPlayer <= 1 && characterStats.MovementRange - lastTilesMoved >= 1)
                    {
                        Debug.LogWarning($"{name} last tile moved {lastTilesMoved}.");
                        yield return AttackWithKnockback(chaseTarget);
                        hasAttacked = true;
                    }
                }

            }
            else
            {
                Debug.Log($"{name} cannot move — all adjacent tiles blocked.");
            }
        }

        yield return new WaitForSeconds(0.1f);
    }


    CharacterStats FindAdjacentPlayer()
    {
        Vector2Int index = GetTileIndices(GetComponent<Tile>().CurrentTileData.transform);
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
                        return stats;
                }
            }
        }

        return null;
    }

    IEnumerator AttemptKnockback(GameObject target)
    {
        _isKnockingBack = true;

        CharacterController controller = target.GetComponent<CharacterController>();
        if (controller == null || controller.GetComponent<Tile>().CurrentTileData == null)
        {
            _isKnockingBack = false;
            yield break;
        }

        Vector2Int from = GetTileIndices(GetComponent<Tile>().CurrentTileData.transform);
        Vector2Int to = GetTileIndices(controller.GetComponent<Tile>().CurrentTileData.transform);

        Vector2Int rawDir = to - from;
        Vector2Int direction = new Vector2Int(
            Mathf.Clamp(rawDir.x, -1, 1),
            Mathf.Clamp(rawDir.y, -1, 1)
        );

        Vector2Int finalPos = to;
        for (int i = 0; i < knockbackRange; i++)
        {
            Vector2Int nextPos = finalPos + direction;
            if (!tileManager.IsWithinBounds(nextPos.x, nextPos.y)) break;

            TileData tile = tileManager.GetTileDataAt(nextPos.x, nextPos.y);
            if (tile == null || tile.IsOccupied) break;

            finalPos = nextPos;
        }

        if (finalPos == to)
        {
            Debug.Log($"{target.name} cannot be knocked back — path blocked.");
            _isKnockingBack = false;
            yield break;
        }

        TileData knockTile = tileManager.GetTileDataAt(finalPos.x, finalPos.y);
        if (knockTile != null)
        {
            Debug.Log($"{target.name} is knocked back to {finalPos}!");

            controller.GetComponent<Tile>().CurrentTileData.ClearOccupant();
            controller.GetComponent<Tile>().CurrentTileData = knockTile;
            knockTile.AssignOccupant(target);

            Vector3 start = target.transform.position;
            Vector3 end = knockTile.transform.position;

            int spacing = 5; // gap between layers (10, 15, 20, etc.)
            Vector2Int tilePos = GetTileIndices(knockTile.transform);
            int sortingOrder = 10 + tilePos.y * spacing;


            end.z = -sortingOrder * 0.01f;

            SpriteRenderer sr = target.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = sortingOrder;
            target.GetComponent<GearEquipper>()?.SetWeaponSL(sortingOrder);


            float distance = Vector3.Distance(start, end);
            float moveTime = distance / Mathf.Max(knockbackSpeed, 0.01f);
            float elapsed = 0f;

            while (elapsed < moveTime)
            {
                float t = elapsed / moveTime;
                t = t * t * (3f - 2f * t); // smoothstep
                target.transform.position = Vector3.Lerp(start, end, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.transform.position = end;
        }

        _isKnockingBack = false;
    }


    #region Animation attack
    private CharacterStats _targetStats;
    private int _remainingAttacks;
    private bool _isKnockingBack = false;

    IEnumerator AttackWithKnockback(CharacterStats target)
    {
        if (target == null || characterStats == null || characterStats.IsDead) yield break;

        _targetStats = target;
        _remainingAttacks = Mathf.Max(1, characterStats.AttackCount);

        // ✅ Face the player
        TileData targetTile = target.GetComponent<CharacterController>()?.GetComponent<Tile>().CurrentTileData;
        if (targetTile != null)
            ScaleCharacter(targetTile);

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

       int damage= _targetStats.TakeDamage(characterStats.attack);

        Debug.Log($"{name} dealt {characterStats.attack} damage to {_targetStats.name}");

        // ✅ Start knockback if still alive
        if (!_targetStats.IsDead && !_isKnockingBack && damage>0)
            StartCoroutine(AttemptKnockback(_targetStats.gameObject));
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
    private int lastTilesMoved = 0;

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
