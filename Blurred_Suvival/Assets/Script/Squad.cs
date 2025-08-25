using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.TextCore.Text;
using System.Linq;

public class Squad : MonoBehaviour
{
    public List<GameObject> Characters;
    public TileManager tileManager;
    public TurnManager turnManager; // 👈 Assign in Inspector

    public GameObject gameOverPanel;
    public Region region;

    public bool SelfEncounter = false;

    public GameObject SaveFormationButton;

    private void OnEnable()
    {
        region.loadBattleGround();
        PlaceCharactersInMatrix();
        if (!SelfEncounter)
        {
            region.TrySpawnEnemies();
        }
        else
        {
            // if (SquadFormationManager.Instance.CheckBornLeader(Characters))
            // {
            //     SaveFormationButton.SetActive(true);
            // }
        }

        // Initialize turn manager after all placements
        if (turnManager != null)
        {
            StartCoroutine(DelayedTurnInitialization());
        }
        else
        {
            Debug.LogError("❌ TurnManager not assigned!");
        }

        SelfEncounter = false;
    }

    void PlaceCharactersInMatrix()
    {
        if (tileManager == null)
        {
            Debug.LogError("TileManager not assigned.");
            return;
        }

        ClearTileOccupants();
        tileManager.AutoTile();

        List<(Transform tile, int row)> validTiles = new List<(Transform, int)>();

        // 3x2 matrix: rows 0–3, cols 0–1
        for (int row = 0; row <= 3; row++)
        {
            for (int col = 0; col <= 1; col++)
            {
                Transform tile = tileManager.tiles[row, col];
                if (tile != null)
                {
                    TileData data = tile.GetComponent<TileData>();
                    if (data == null)
                    {
                        Debug.LogError($"❌ Tile[{row},{col}] missing TileData component!");
                        continue;
                    }

                    if (!data.IsOccupied)
                    {
                        validTiles.Add((tile, row));
                        Debug.Log($"✅ Tile[{row},{col}] added to valid list.");
                    }
                    else
                    {
                        Debug.Log($"🚫 Tile[{row},{col}] is already occupied.");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ Tile[{row},{col}] is null.");
                }
            }
        }

        if (validTiles.Count == 0)
        {
            Debug.LogError("No valid tiles found in 3x2 area.");
            return;
        }

        validTiles.Shuffle(); // Ensure randomness

        int spacing = 5; // gap between layers

        for (int i = 0; i < Characters.Count && i < validTiles.Count; i++)
        {
            GameObject character = Characters[i];
            if (character == null) continue;

            var (tile, row) = validTiles[i];

            // Compute sorting order with spacing
            int sortingOrder = 10 + row * spacing;

            // Assign world position (z-depth for pseudo-3D layering)
            Vector3 tilePos = tile.position;
            tilePos.z = -sortingOrder * 0.01f; // optional small offset for overlap
            character.transform.position = tilePos;

            // Update weapon sorting if character has controller
            CharacterController controller = character.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.SetWeaponSL(sortingOrder);
                controller.tileManager = tileManager;
                controller.currentTileData = tile.GetComponent<TileData>();
                controller.turnManager = turnManager;
            }

            character.SetActive(true);

            // Assign tile occupant
            TileData data = tile.GetComponent<TileData>();
            if (data != null)
            {
                data.AssignOccupant(character);
                Debug.Log($"👣 Assigned {character.name} to Tile[{row},?]");
            }

            // Set sprite sorting order
            SpriteRenderer sr = character.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = sortingOrder;
            }
        }

    }

    private IEnumerator DelayedTurnInitialization()
    {
        yield return null; // wait one frame

        if (turnManager != null)
        {
            turnManager.InitializeCharacters();
            turnManager.BeginPlayerTurn();
        }
    }

    void ClearTileOccupants()
    {
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 16; col++)
            {
                Transform tile = tileManager.tiles[row, col];
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

    public void CheckIfAllPlayersDead()
    {
        foreach (GameObject character in Characters)
        {
            if (character != null)
            {
                CharacterStats stats = character.GetComponent<CharacterStats>();
                if (stats != null && !stats.IsDead)
                {
                    return;
                }
            }
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("All players are dead! Game Over panel activated.");
        }
    }
}
