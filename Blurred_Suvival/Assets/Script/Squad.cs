using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Squad : MonoBehaviour
{
    public List<GameObject> Characters;
    public TileManager tileManager;
    public TurnManager turnManager; // Assign in Inspector

    public GameObject gameOverPanel;
    public GameObject SaveFormationButton;
    public static Squad Instance;

    [Header("Squad Attribute")]
    public float RetreatChance = 25f;
    public float AmbushChance = 25f;
    public float EncounterChance = 50f;
    public int MaxSurvivorCountInGroup = 12;

    public Enemy enemy;
    public BattleManager battleManager;

    Region region;

    public TextMeshProUGUI SquadNumberText;

    [HideInInspector]
    public Action EventLootGeneratorByName;

    private void OnEnable()
    {
        if (Instance == null) Instance = this;
    }

    public void SetSquadNumberText()
    {
        SquadNumberText.text = $"{transform.childCount}/{MaxSurvivorCountInGroup}";
    }

    #region Battle Initiation

    /// <summary>
    /// Enqueue a battle through the centralized BattleQueueManager
    /// </summary>
    public void RequestBattle(bool IsEvent, Event eventData, Region region, bool SelfEncounter, GameObject BattleField = null)
    {
        BattleQueueManager.Instance.EnqueueBattle(IsEvent, eventData, region, SelfEncounter, BattleField);
    }

    public void InitiateBattleInternal(bool IsEvent, Event eventData, Region region, bool SelfEncounter, GameObject BattleField = null)
    {
        this.region = region;
        CheckCharacterListAnamoly();
        enemy.CheckSpawnListAnamoly();

        region.loadBattleGround(BattleField);

        InitializeCharacters();
        PlaceCharactersInMatrix();

        if (IsEvent)
        {
            MusicManager.Instance.PlayDramaticMusic();
            RetreatHandler.CanRetreat = eventData.AllowRetreat;
            region.TrySpawnEventTriggerEnemies(turnManager, eventData.Survivors);
            TurnManager.Instance.StartEventTrigger(eventData);
        }
        else
        {
            EncounterWithEnemy(SelfEncounter);
        }

        battleManager.CalculateTotalXP();
    }

    #endregion

    void EncounterWithEnemy(bool SelfEncounter)
    {
        if (!SelfEncounter)
        {
            MusicManager.Instance.PlayBattleMusic();
            float roll = UnityEngine.Random.Range(0f, 100f);

            if (roll <= AmbushChance)
            {
                turnManager.EncounterMode = 0;
                region.TrySpawnAmbushEnemies(turnManager);
            }
            else if (roll <= (AmbushChance + EncounterChance))
            {
                turnManager.EncounterMode = 1;
                region.TrySpawnEnemies(turnManager);
            }
            else
            {
                turnManager.EncounterMode = 2;
                region.TrySpawnPreemptiveEnemies(turnManager);
            }
        }
        else
        {
            MusicManager.Instance.PlayDramaticMusic();
            turnManager.EncounterMode = 3;
        }

        if (turnManager != null)
        {
            StartCoroutine(DelayedTurnInitialization());
        }
        else
        {
            Debug.LogError("❌ TurnManager not assigned!");
        }
    }

    #region Character Management

    void CheckCharacterListAnamoly() => Characters.RemoveAll(character => character == null);

    public void InitializeCharacters()
    {
        Characters.Clear();
        foreach (Transform child in transform) Characters.Add(child.gameObject);
        SetSquadNumberText();
    }

    void PlaceCharactersInMatrix()
    {
        if (tileManager == null)
        {
            Debug.LogError("TileManager not assigned.");
            return;
        }

        tileManager.ClearTileOccupants();
        tileManager.AutoTile();

        EventLootGeneratorByName?.Invoke();

        List<(Transform tile, int row)> validTiles = new List<(Transform, int)>();

        // 3x2 matrix: rows 0–3, cols 0–3
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

        validTiles.Shuffle();

        int spacing = 5;

        for (int i = 0; i < Characters.Count && i < validTiles.Count; i++)
        {
            GameObject character = Characters[i];
            if (character == null) continue;

            var (tile, row) = validTiles[i];
            int sortingOrder = 10 + row * spacing;

            CharacterController controller = character.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.turnManager = turnManager;
                controller.currentTileData = tile.GetComponent<TileData>();
                controller.ResetScale();
                controller.GetComponent<GearEquipper>()?.SetWeaponSL(sortingOrder);
            }

            Vector3 tilePos = tile.position;
            tilePos.z = -sortingOrder * 0.01f;
            character.transform.position = tilePos;

            character.GetComponent<TurnIndicator>().SetIndicator(false);
            character.SetActive(true);

            tile.GetComponent<TileData>()?.AssignOccupant(character);

            SpriteRenderer sr = character.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = sortingOrder;
        }
    }

    private IEnumerator DelayedTurnInitialization()
    {
        yield return null;
        turnManager?.StartBattle();
    }

    #endregion

    #region Player Death & Removal

    public void CheckIfAllPlayersDead()
    {
        foreach (GameObject character in Characters)
        {
            if (character != null)
            {
                CharacterStats stats = character.GetComponent<CharacterStats>();
                if (stats != null && !stats.IsDead) return;
            }
        }

        StartCoroutine(LoadGameOverScreen());
    }

    IEnumerator LoadGameOverScreen()
    {
        yield return new WaitForSeconds(1.5f);
        MusicManager.Instance.PlayDramaticMusic();

        CutsceneManager.Instance.cutsceneID = "Game over";
        CutsceneManager.Instance.PlayCutsceneByID(gameOverPanel);
    }

    public void RemoveCharacter(GameObject character)
    {
        TurnManager.Instance.RemovePlayer(character.GetComponent<CharacterController>());
        Characters.Remove(character);
        SetSquadNumberText();
    }

    #endregion

    #region Retreat

    public bool RetreatSuccess;

    public float GetRetreatChance() => RetreatChance / Characters.Count;

    public void Retreat()
    {
        if (RetreatHandler.CanRetreat)
        {
            RetreatSuccess = UnityEngine.Random.Range(0f, 100f) < GetRetreatChance();
        }
        else
        {
            RetreatSuccess = false;
        }
        Debug.Log(RetreatSuccess ? "✅ Retreat successful!" : "❌ Retreat failed!");
    }

    public void LoadRetreatAnimationForAll(GameObject TriggerObj)
    {
        foreach (GameObject obj in Characters)
        {
            if (obj != TriggerObj)
                StartCoroutine(obj.GetComponent<CharacterController>().OnRetreatAll(true));
        }
    }
    #endregion
}
