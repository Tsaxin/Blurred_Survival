using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ZombieAIBase : MonoBehaviour
{
    public TileManager tileManager;
    public abstract IEnumerator TakeTurn();
    public Enemy EnemyManager;

    public CharacterStats characterStats;

    public TrailRenderer trailRenderer;

    public float ScaleForZombie = 1;

    public virtual Vector2Int GetTileIndices(Transform tileTransform)
    {
        for (int row = 0; row < tileManager.tiles.GetLength(0); row++)
        {
            for (int col = 0; col < tileManager.tiles.GetLength(1); col++)
            {
                if (tileManager.tiles[row, col] == tileTransform)
                    return new Vector2Int(col, row);
            }
        }
        return new Vector2Int(-1, -1);
    }

    public virtual void Initialize(TileManager manager, TileData startTile)
    {
        tileManager = manager;
        GetComponent<Tile>().CurrentTileData = startTile;

        if (GetComponent<Tile>().CurrentTileData != null)
        {
            GetComponent<Tile>().CurrentTileData.AssignOccupant(gameObject);
        }
    }

    // 🔹 Shared MoveToTile for all zombies
    protected IEnumerator MoveToTile(TileData targetTile)
    {
        if (targetTile == null) yield break;

        GetComponent<Tile>().CurrentTileData.ClearOccupant();
        ScaleCharacter(targetTile);

        Vector3 start = transform.position;
        Vector3 end = targetTile.transform.position;

        float moveTime = 0.25f;
        float elapsed = 0f;

        while (elapsed < moveTime)
        {
            float t = elapsed / moveTime;
            t = t * t * (3f - 2f * t); // smoothstep
            transform.position = Vector3.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        SortingOrder(targetTile.gameObject);

        GetComponent<Tile>().CurrentTileData = targetTile;
        targetTile.AssignOccupant(gameObject);

        Debug.Log($"{name} moved to new tile.");
    }

    public void SortingOrder(GameObject targetTile)
    {
        int spacing = 5; // gap between layers
        int sortingOrder = 10 + GetTileIndices(targetTile.transform).y * spacing;
        if (trailRenderer != null) trailRenderer.sortingOrder = sortingOrder + 1;

        // Get current position
        Vector3 pos = transform.position;

        // Update Z based on sorting order
        pos.z = -sortingOrder * 0.01f;

        transform.position = pos; // apply it

        // Update sprite renderer sorting order
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = sortingOrder;
    }

    public void ScaleCharacter(TileData targetTile)
    {
        int ResultScale = TileManager.Instance.GetXDirection(GetComponent<Tile>().CurrentTileData.transform, targetTile.transform);
        transform.localScale = new Vector3(-1 * ScaleForZombie * ResultScale, transform.localScale.y, transform.localScale.z);

        Canvas childCanvas = GetComponentInChildren<Canvas>();
        if (childCanvas != null)
        {
            childCanvas.transform.localScale = new Vector3(-1 * ScaleForZombie * ResultScale * Mathf.Abs(childCanvas.transform.localScale.x), childCanvas.transform.localScale.y, childCanvas.transform.localScale.z);
        }
    }

    public void ScaleCharacter(int Scale)   //minus value means facing right
    {
        Scale = -1 * Scale;
        transform.localScale = new Vector3(-1 * Scale, transform.localScale.y, transform.localScale.z);

        Canvas childCanvas = GetComponentInChildren<Canvas>();
        if (childCanvas != null)
        {
            childCanvas.transform.localScale = new Vector3(-1 * Scale * Mathf.Abs(childCanvas.transform.localScale.x), childCanvas.transform.localScale.y, childCanvas.transform.localScale.z);
        }
    }

    // 🔹 Shared pathfinding + chase logic
    protected TileData FindChasingTileAndAttackOpportunity(out CharacterStats attackTarget)
    {
        attackTarget = null;

        Vector2Int myIndex = GetTileIndices(GetComponent<Tile>().CurrentTileData.transform);
        CharacterStats nearestPlayer = null;
        float minDistance = float.MaxValue;
        Vector2Int playerIndex = Vector2Int.zero;

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            CharacterStats stats = obj.GetComponent<CharacterStats>();
            if (stats != null && !stats.IsDead)
            {
                TileData playerTile = stats.GetComponent<CharacterController>()?.GetComponent<Tile>().CurrentTileData;
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

                if (distFromCurrent == 1)
                    allAdjacentBlocked = false;
            }
        }

        if (movableTiles.Count == 0 || allAdjacentBlocked)
        {
            Debug.Log($"{name} cannot move — all adjacent tiles blocked.");
            return null;
        }

        // --- Choose best tile using Chebyshev distance (primary),
        // then Manhattan (secondary), then Euclidean (tertiary) ---
        TileData bestTile = null;
        int bestChebyshev = int.MaxValue;
        int bestManhattan = int.MaxValue;
        int playerX = playerIndex.x;
        int playerY = playerIndex.y;
        float bestSqrEuclid = float.MaxValue;

        foreach (TileData tile in movableTiles)
        {
            Vector2Int pos = GetTileIndices(tile.transform);
            int cheb = Mathf.Max(Mathf.Abs(pos.x - playerX), Mathf.Abs(pos.y - playerY));
            int man = Mathf.Abs(pos.x - playerX) + Mathf.Abs(pos.y - playerY);
            float sqrEuc = (pos - playerIndex).sqrMagnitude; // use squared euclid for tie-breaker

            if (cheb < bestChebyshev)
            {
                bestChebyshev = cheb;
                bestManhattan = man;
                bestSqrEuclid = sqrEuc;
                bestTile = tile;
            }
            else if (cheb == bestChebyshev)
            {
                if (man < bestManhattan || (man == bestManhattan && sqrEuc < bestSqrEuclid))
                {
                    bestManhattan = man;
                    bestSqrEuclid = sqrEuc;
                    bestTile = tile;
                }
            }
        }

        return bestTile;
    }
}
