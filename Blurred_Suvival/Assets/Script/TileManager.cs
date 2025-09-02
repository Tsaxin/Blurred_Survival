using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance;
    public Transform[] flatTileList; // Drag tiles in column-major order: down rows, then right columns
    public Transform[,] tiles = new Transform[4, 16];

    public Transform LeftRetreatTile, RightRetreatTile;

    void Start()
    {
        if (Instance == null)
            Instance = this;
    }


    public void ClearTileOccupants()
    {
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 16; col++)
            {
                Transform tile = tiles[row, col];
                if (tile != null)
                {
                    TileData data = tile.GetComponent<TileData>();
                    if (data != null)
                    {
                        data.ClearOccupant();
                    }
                }
            }
        }
    }


    private List<TileData> highlightedTiles = new List<TileData>();

    public void HighlightTile(TileData tile, bool isRange = false)
    {
        if (isRange)
            tile.ShowRangeColor();
        else
            tile.ShowAsPossibleMove();

        if (!highlightedTiles.Contains(tile))
            highlightedTiles.Add(tile);
    }

    public void ClearHighlightedTiles()
    {
        foreach (TileData tile in highlightedTiles)
            tile.ResetColor();
        highlightedTiles.Clear();
    }

    [ContextMenu("AutoTile")]
    public void AutoTile()
    {
        if (flatTileList.Length != 4 * 16)
        {
            Debug.LogError("Expected 64 tiles for 4x16 grid, but got " + flatTileList.Length);
            return;
        }

        for (int i = 0; i < flatTileList.Length; i++)
        {
            int col = i / 4;
            int row = i % 4;

            tiles[row, col] = flatTileList[i];
            if (tiles[row, col] == null)
            {
                Debug.LogError($"Tile at flat index {i} resulted in null at tiles[{row},{col}]");
            }
            else
            {
                tiles[row, col].name = $"Tile[{row},{col}]";
            }
        }

        Debug.Log("TileManager: Tiles stored and renamed.");
    }

    [ContextMenu("Check Tiles")]
    public void DebugCheckTiles()
    {
        bool allGood = true;

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 16; col++)
            {
                if (tiles[row, col] == null)
                {
                    Debug.LogWarning($"❌ Tile[{row},{col}] is NULL!");
                    allGood = false;
                }
                else
                {
                    Debug.Log($"✅ Tile[{row},{col}] is assigned to: {tiles[row, col].name}");
                }
            }
        }

        if (allGood)
        {
            Debug.Log("🎉 All tiles are properly assigned!");
        }
        else
        {
            Debug.LogError("⚠️ Some tiles are still missing!");
        }
    }

    public List<Transform> GetSurroundingTiles(Vector2Int center, int radius)
    {
        List<Transform> result = new List<Transform>();

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x == 0 && y == 0) continue;

                int row = center.y + y;
                int col = center.x + x;

                if (row >= 0 && row < tiles.GetLength(0) &&
                    col >= 0 && col < tiles.GetLength(1))
                {
                    Transform tile = tiles[row, col];
                    if (tile != null)
                        result.Add(tile);
                }
            }
        }

        return result;
    }


    public bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < tiles.GetLength(1) && y >= 0 && y < tiles.GetLength(0);
    }

    public Vector2Int GetTileIndices(Transform tileTransform)
    {
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 16; col++)
            {
                if (tiles[row, col] == tileTransform)
                {
                    return new Vector2Int(col, row);
                }
            }
        }
        Debug.LogWarning("Tile not found in TileManager.");
        return new Vector2Int(-1, -1);
    }

    public TileData GetTileDataAt(int x, int y)
    {
        if (IsWithinBounds(x, y))
        {
            return tiles[y, x].GetComponent<TileData>();
        }
        return null;
    }

    public int GetXDirection(Transform currentTile, Transform targetTile)
    {
        Vector2Int current = GetTileIndices(currentTile);
        Vector2Int target = GetTileIndices(targetTile);

        if (current.x == -1 || target.x == -1)
        {
            Debug.LogWarning("Invalid tile passed to GetXDirection.");
            return 0; // default when tile is not found
        }

        if (target.x < current.x) return -1; // left
        if (target.x > current.x) return 1;  // right
        return 1; // same column
    }

    public float PaddingX, PaddingY, RowSkewX, RowSkewY;
    public Transform TileParent;
    [ContextMenu("Auto Space")]
    [ContextMenu("Auto Space")]
    public void AutoSpace()
    {
        int rows = 4;
        int cols = 16;

        if (TileParent.childCount != rows * cols)
        {
            Debug.LogError($"Expected {rows * cols} tiles but found {TileParent.childCount}");
            return;
        }

        // Anchor = first tile (tile0.0)
        Vector3 origin = TileParent.GetChild(0).position;

        int index = 0;
        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                Transform tile = TileParent.GetChild(index);

                // Normal grid offset
                float offsetX = col * PaddingX;
                float offsetY = row * PaddingY;

                // Additional shift for "parallelogram" effect
                float rowOffsetX = row * RowSkewX;  // <-- new variable
                float rowOffsetY = row * RowSkewY;  // optional (usually 0)

                tile.position = new Vector3(
                    origin.x + offsetX + rowOffsetX,
                    origin.y + offsetY + rowOffsetY,
                    tile.position.z
                );

                index++;
            }
        }

        Debug.Log("✅ AutoSpace with row offset complete.");
    }

    public TileData GetBestTileForDrop()
    {
        List<TileData> candidates = new List<TileData>();

        int minLoot = int.MaxValue;

        for (int row = 0; row < tiles.GetLength(0); row++)
        {
            for (int col = 0; col < tiles.GetLength(1); col++)
            {
                Transform t = tiles[row, col];
                if (t == null) continue;

                TileData td = t.GetComponent<TileData>();
                if (td == null) continue;

                if (td.IsOccupied) continue; // skip occupied tiles

                int lootCount = td.lootOnTile.Count;
                if (lootCount < minLoot)
                {
                    // Found a new "least loot" tile → reset candidate list
                    minLoot = lootCount;
                    candidates.Clear();
                    candidates.Add(td);
                }
                else if (lootCount == minLoot)
                {
                    // Same as current best → add to candidates
                    candidates.Add(td);
                }
            }
        }

        if (candidates.Count == 0)
            return null; // no valid tile

        // Pick random among best candidates
        return candidates[Random.Range(0, candidates.Count)];
    }

    public Transform GetRandomTile(int minColumnIndex = 0)
    {
        List<Transform> validTiles = new List<Transform>();

        for (int row = 0; row < tiles.GetLength(0); row++)
        {
            for (int col = minColumnIndex; col < tiles.GetLength(1); col++)
            {
                Transform t = tiles[row, col];
                if (t != null)
                    validTiles.Add(t);
            }
        }

        if (validTiles.Count == 0)
        {
            Debug.LogWarning($"No valid tiles found with minColumnIndex {minColumnIndex}");
            return null;
        }

        return validTiles[Random.Range(0, validTiles.Count)];
    }


}
