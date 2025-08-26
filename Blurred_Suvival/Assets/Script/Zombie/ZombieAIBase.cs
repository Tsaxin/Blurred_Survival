using UnityEngine;
using System.Collections;

public abstract class ZombieAIBase : MonoBehaviour
{
    public TileManager tileManager;
    public TileData currentTileData;

    public abstract IEnumerator TakeTurn();

    public string ZombieName;
    public Enemy EnemyManager;

    public TrailRenderer trailRenderer;

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
        currentTileData = startTile;

        if (currentTileData != null)
        {
            currentTileData.AssignOccupant(gameObject);
        }
    }

    // 🔹 Shared MoveToTile for all zombies
    protected IEnumerator MoveToTile(TileData targetTile)
    {
        if (targetTile == null) yield break;

        currentTileData.ClearOccupant();
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

        currentTileData = targetTile;
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
        int ResultScale = TileManager.Instance.GetXDirection(currentTileData.transform, targetTile.transform);
        transform.localScale = new Vector3(-1 * ResultScale, transform.localScale.y, transform.localScale.z);

        Canvas childCanvas = GetComponentInChildren<Canvas>();
        if (childCanvas != null)
        {
            childCanvas.transform.localScale = new Vector3(-1 * ResultScale * Mathf.Abs(childCanvas.transform.localScale.x), childCanvas.transform.localScale.y, childCanvas.transform.localScale.z); // example scale
        }
    }
    
    public void ScaleCharacter(int Scale)   //minus value means facing right
    {
        Scale = -1 * Scale; 
        transform.localScale = new Vector3(-1 * Scale, transform.localScale.y, transform.localScale.z);

        Canvas childCanvas = GetComponentInChildren<Canvas>();
        if (childCanvas != null)
        {
            childCanvas.transform.localScale = new Vector3(-1 * Scale * Mathf.Abs(childCanvas.transform.localScale.x), childCanvas.transform.localScale.y, childCanvas.transform.localScale.z); // example scale
        }
    }
}
