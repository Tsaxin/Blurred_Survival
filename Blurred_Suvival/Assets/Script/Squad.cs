using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.TextCore.Text;

public class Squad : MonoBehaviour
{
    public List<GameObject> Characters;
    public TileManager tileManager;
    public TurnManager turnManager; // 👈 Assign in Inspector

    public GameObject gameOverPanel;
    public Region region;

    public bool SelfEncounter = false;

    public GameObject SaveFormationButton;
    public static Squad Instance;

    [Header("Squad Attribute")]
    public float RetreatChance = 25f;
    public float AmbushChance = 25;
    public float EncounterChance = 50f;

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        CheckCharacterListAnamoly();

        region.loadBattleGround();
        PlaceCharactersInMatrix();
        if (!SelfEncounter)
        {
            float roll = Random.Range(0f, 100f);

            if (roll <= AmbushChance)
            {
                turnManager.EncounterMode = 0;
                region.TrySpawnAmbushEnemies();
            }
            else if (roll <= (AmbushChance + EncounterChance))
            {
                turnManager.EncounterMode = 1;
                region.TrySpawnEnemies();
            }
            else
            {
                turnManager.EncounterMode = 2;
                region.TrySpawnPreemtiveEnemies();
            }
        }
        else
        {
            turnManager.EncounterMode = 3;
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

    void CheckCharacterListAnamoly()
    {
        Characters.RemoveAll(character => character == null);
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
            for (int col = 0; col <= 3; col++)
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
                controller.currentTileData = tile.GetComponent<TileData>();
                controller.turnManager = turnManager;
                controller.ResetScale();
            }

            character.GetComponent<TurnIndicator>().SetIndicator(false);
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
            turnManager.StartBattle();
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

    public void RemoveCharacter(GameObject character)
    {
        //Also removing from turnmanager
        TurnManager.Instance.RemovePlayer(character.GetComponent<CharacterController>());
        if (Characters.Contains(character))
        {
            Characters.Remove(character);
            Debug.Log($"🗑 Removed {character.name} from squad list.");
        }

        //Removing character from hunger manager as well
    }

    public float GetRetreatChance()
    {
        float RetreatChance = this.RetreatChance / Characters.Count;

        return RetreatChance;
    }

    public bool RetreatSuccess;
    public void Retreat()
    {
        RetreatSuccess = false;
        float retreatChance = GetRetreatChance(); // e.g., 25 means 25%

        // Roll a random number between 0 and 100
        float roll = Random.Range(0f, 100f);

        if (roll < retreatChance)
        {
            Debug.Log("✅ Retreat successful!");
            RetreatSuccess = true;
        }
        else
        {
            RetreatSuccess = false;
        }
    }

    public void LoadRetreatAnimationForAll(GameObject TriggerObj)
    {
        foreach (GameObject obj in Characters)
        {
            if (obj != TriggerObj)
            {
                StartCoroutine(obj.GetComponent<CharacterController>().OnRetreatAll());
            }
        }
    }

}
