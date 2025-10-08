using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GameObject playerParent; // 👈 Assign this in Inspector
    public GameObject enemyParent;  // 👈 Assign this in Inspector
    public List<ZombieAIBase> enemyCharacters;

    private int currentCharacterIndex = 0;
    private bool playerTurn = true;

    [Header("Turn Timing")]
    public float delayBetweenEnemyMoves = 0.2f; // Adjust in Inspector

    [Header("Post-Battle Settings")]
    public GameObject PostBattlePanel;
    public GameObject CampButton,ShortCutParent;

    public List<CharacterController> activePlayerCharacters = new List<CharacterController>();

    public static TurnManager Instance;

    public CharacterController SelectedUnit;

    [Header("Encounter Mode")]
    public CanvasGroup EncounterModeCanvasGroup;
    public TextMeshProUGUI EncounterTypeText, EncounterDescription;
    public float EncounterPanelFadeSpeed, EncounterPanelStaySpeed;
    public int EncounterMode; //0=Ambush,1=Encounter,2=Pre emtive,3=Camp mode
    private bool preemptiveExtraTurnPending = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Prevent duplicates
    }

    public bool IsPlayerTurn()
    {
        return playerTurn;
    }
    public void InitializeCharacters()
    {
        if (playerParent != null)
        {
            playerParent.GetComponent<Squad>().InitializeCharacters();
        }
        else
        {
            Debug.LogError("❌ PlayerParent not assigned in TurnManager.");
        }

        if (enemyParent != null)
        {
            enemyCharacters = new List<ZombieAIBase>(enemyParent.GetComponentsInChildren<ZombieAIBase>());
        }
        else
        {
            Debug.LogError("❌ EnemyParent not assigned in TurnManager.");
        }
    }

    public void StartBattle()
    {
        CampButton.SetActive(false);
        ShortCutParent.SetActive(true);
        InitializeCharacters();
        SetHasMoveState(playerParent.transform,true);
        SetHasMoveState(enemyParent.transform,true);

        switch (EncounterMode)
        {
            case 0: // Ambush → Enemy starts
                CoroutineRunner.Instance.StartCoroutine(HandleAmbush());
                break;

            case 1: // Encounter → Player starts
                CoroutineRunner.Instance.StartCoroutine(HandleRandomEncounter());
                break;

            case 2: // Preemptive → Player gets 2 turns before enemy
                CoroutineRunner.Instance.StartCoroutine(HandlePreemptiveTurn());
                break;
            case 3:
                CoroutineRunner.Instance.StartCoroutine(HandleCamp());
                break;
        }
    }

    void SetHasMoveState(Transform parent,bool State)
    {
        foreach (Transform child in parent)
        {
            if (child.GetComponent<CharacterController>() != null)
            {
                child.GetComponent<CharacterController>().hasMoved = State;
            }
        }
    }

    IEnumerator HandleAmbush()
    {
        yield return LoadCanvasGroup("Ambush", "Enemy strike first!");
        StartCoroutine(BeginEnemyTurn());
    }

    IEnumerator HandleRandomEncounter()
    {
        yield return LoadCanvasGroup("Random Encounter", "Survivors strike first!");
        BeginPlayerTurn();
    }

    private IEnumerator HandlePreemptiveTurn()
    {
        yield return LoadCanvasGroup("Preemptive Strike", "Survivors strike first and get 2 two turns!");

        preemptiveExtraTurnPending = true; // tell manager to skip enemy after first turn
        BeginPlayerTurn();

        // Wait until first player phase fully finishes
        yield return new WaitUntil(() => playerTurn == false);

        Debug.Log("⚡ Extra Preemptive Player Turn!");
        BeginPlayerTurn();

        // Wait until second player phase finishes
        yield return new WaitUntil(() => playerTurn == false);
    }

    IEnumerator HandleCamp()
    {
        yield return LoadCanvasGroup("Survivor Camp", "Survivors can do what they want!");
        BeginPlayerTurn();
    }
    public void BeginPlayerTurn()
    {
        GetComponent<GlobalTurnIndicator>().GlobalTurnIndicatorState(true, "Player's turn");
        playerTurn = true;
        currentCharacterIndex = 0;

        activePlayerCharacters = new List<CharacterController>(playerParent.GetComponentsInChildren<CharacterController>());
        // Get only alive, existing players

        LoadEveryTurnPassive(activePlayerCharacters);

        foreach (var pc in activePlayerCharacters)
        {
            pc.BeginTurn();
        }

        Debug.Log($"▶️ Player Turn Begins with {activePlayerCharacters.Count} active characters");

        // Skip to enemy turn if no players are alive
        if (activePlayerCharacters.Count == 0)
        {
            return;
        }
    }

    public void OnPlayerFinishedMove(CharacterController character)
    {
        if (!activePlayerCharacters.Contains(character)) return;

        currentCharacterIndex++;

        Debug.Log($"➡️ Player {currentCharacterIndex}/{activePlayerCharacters.Count} moved.");

        if (currentCharacterIndex >= activePlayerCharacters.Count)
        {
            if (preemptiveExtraTurnPending)
            {
                // Skip enemy turn ONCE, clear flag
                preemptiveExtraTurnPending = false;
                playerTurn = false; // release WaitUntil in coroutine
            }
            else
            {
                Debug.Log("This triggered");
                CoroutineRunner.Instance.StartCoroutine(BeginEnemyTurn());
            }
        }
    }

    [Header("Turn Timing")]
    public float delayBeforeEnemyTurn = 0.1f;   // 👈 New field
    IEnumerator BeginEnemyTurn()
    {
        GetComponent<GlobalTurnIndicator>().GlobalTurnIndicatorState(true,"Enemy's turn");
        playerTurn = false;
        Debug.Log("⏳ Waiting before enemy turn...");
        yield return new WaitForSeconds(delayBeforeEnemyTurn);

        Debug.Log("🔁 Enemy Turn Begins");

        enemyCharacters.RemoveAll(e => e == null);

        // Sort for consistent order
        enemyCharacters.Sort((a, b) =>
        {
            Vector2Int aIndex = a.GetTileIndices(a.GetComponent<Tile>().CurrentTileData.transform);
            Vector2Int bIndex = b.GetTileIndices(b.GetComponent<Tile>().CurrentTileData.transform);
            int colCompare = aIndex.x.CompareTo(bIndex.x);
            return colCompare != 0 ? colCompare : aIndex.y.CompareTo(bIndex.y);
        });

        // 🧠 Snapshot current positions at the beginning of the turn
        List<ZombieAIBase> enemiesToAct = new List<ZombieAIBase>(enemyCharacters);

        foreach (var enemy in enemiesToAct)
        {
            if (enemy != null && !enemy.GetComponent<CharacterStats>().IsDead)
            {
                yield return enemy.TakeTurn(); // this should run only once
                yield return new WaitForSeconds(delayBetweenEnemyMoves);
            }
        }

        yield return new WaitForSeconds(0.25f);
        BeginPlayerTurn();
    }
    // Optional: Call this from CharacterStats when zombie dies

    public BattleManager BattleManager;
    public void RemoveEnemy(ZombieAIBase enemy)
    {
        // Remove the enemy if it's in the list
        if (enemyCharacters.Contains(enemy))
        {
            enemyCharacters.Remove(enemy);
        }

        // Clean up any null/missing entries
        enemyCharacters.RemoveAll(e => e == null);

        // Check if all enemies are gone
        if (enemyCharacters.Count == 0)
        {
            Debug.Log("🎉 All enemies defeated! Preparing to exit encounter...");

            // Distribute XP
            if (BattleManager != null)
            {
                BattleManager.DistributeXPToAlivePlayers();
            }
            else
            {
                Debug.LogWarning("⚠️ BattleManager not found when trying to distribute XP.");
            }

            // Show post battle UI
            PostBattlePanel.SetActive(true);
        }
    }

    void LoadEveryTurnPassive(List<CharacterController> CC)
    {
        foreach (var character in CC)
        {
            character.GetComponent<CharacterPassive>().TriggerTurnStartEffects();
        }
    }

    #region encounterCanvasGroup

    IEnumerator LoadCanvasGroup(string Type, string Description)
    {
        EncounterTypeText.text = Type;
        EncounterDescription.text = Description;

        EncounterModeCanvasGroup.gameObject.SetActive(true);

        // Fade in
        yield return StartCoroutine(FadeCanvasGroup(EncounterModeCanvasGroup, 0f, 1f, EncounterPanelFadeSpeed));

        // Stay visible for 1 sec
        yield return new WaitForSeconds(EncounterPanelStaySpeed);

        // Fade out
        yield return StartCoroutine(FadeCanvasGroup(EncounterModeCanvasGroup, 1f, 0f, EncounterPanelFadeSpeed));
        EncounterModeCanvasGroup.gameObject.SetActive(false);

    }
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float t = 0f;
        cg.alpha = start;
        cg.gameObject.SetActive(true);

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        cg.alpha = end;

        if (end == 0f)
            cg.gameObject.SetActive(false);
    }

    public void RemovePlayer(CharacterController player)
    {
        if (activePlayerCharacters.Contains(player))
        {
            int indexOfRemoved = activePlayerCharacters.IndexOf(player);

            activePlayerCharacters.Remove(player);

            // Adjust currentCharacterIndex if necessary
            if (indexOfRemoved <= currentCharacterIndex && currentCharacterIndex > 0)
                currentCharacterIndex--;

            Debug.Log($"🗑 Removed {player.name} from squad list.");
        }

        // Check if turn should end
        if (currentCharacterIndex >= activePlayerCharacters.Count)
        {
            playerTurn = false; // ends WaitUntil
            Debug.Log("All remaining players done or dead. Ending player turn.");
        }
    }

    #endregion

    #region Event Trigger 
    public void StartEventTrigger(Event EventData)
    {
        CampButton.SetActive(false);
        ShortCutParent.SetActive(true);
        SetHasMoveState(playerParent.transform,true);
        SetHasMoveState(enemyParent.transform,true);
        CoroutineRunner.Instance.StartCoroutine(LoadEventTriggerDetails(EventData));
    }

    IEnumerator LoadEventTriggerDetails(Event EventData)
    {
        yield return LoadCanvasGroup("Interaction", "Survivor interacts first!");
        List<GameObject> enemyChildren = new List<GameObject>();
        for (int i = 0; i < enemyParent.transform.childCount; i++)
        {
            enemyChildren.Add(enemyParent.transform.GetChild(i).gameObject);
        }

        // Pass the full list to the dialogue manager
        DialogueManager.Instance.InitiateDialogue(EventData, enemyChildren);
    }

    public void SetNPCAsEnemy()
    {
        MusicManager.Instance?.PlayBattleMusic();

        InitializeCharacters();

        Enemy enemy = enemyParent.GetComponent<Enemy>();
        foreach (GameObject child in enemy.spawnedEnemies)
        {
            if (child.GetComponent<CharacterController>() != null) child.GetComponent<CharacterController>().enabled = false;
            child.gameObject.tag = "Enemy";
            child.GetComponentInChildren<SpriteRenderer>().gameObject.tag = "Enemy";

            if (child.GetComponent<ZombieAIBase>() != null)
            {
                child.GetComponent<ZombieAIBase>().characterStats = child.GetComponent<CharacterStats>();
                child.GetComponent<ZombieAIBase>().ScaleForZombie = -1;
                child.GetComponent<ZombieAIBase>().ScaleCharacter(-1);
            }
        }
        int rand = Random.Range(0, 2);   //0 being player moves first
        if (rand == 0)
        {
            BeginPlayerTurn();
        }
        else
        {
            StartCoroutine(BeginEnemyTurn());
        }
    }

    public void NPCRunAway()
    {
        TurnExploreModeOn();
        Enemy enemy = enemyParent.GetComponent<Enemy>();
        foreach (GameObject child in enemy.spawnedEnemies)
        {
            CoroutineRunner.Instance.StartCoroutine(child.GetComponent<CharacterController>().OnRetreatAll(false));
        }

        PostBattlePanel.SetActive(true);
    }

    public void NPCDropLootAndRunAway()
    {
        TurnExploreModeOn();

        Enemy enemy = enemyParent.GetComponent<Enemy>();
        foreach (GameObject child in enemy.spawnedEnemies)
        {
            LootMasterManager.Instance.DropRandomLoots(child.GetComponent<ZombieAIBase>().GetComponent<Tile>().CurrentTileData);
            CoroutineRunner.Instance.StartCoroutine(child.GetComponent<CharacterController>().OnRetreatAll(false));
        }
    }

    public void NPCJoin()
    {
        if ((playerParent.transform.childCount + enemyParent.transform.childCount) <= playerParent.GetComponent<Squad>().MaxSurvivorCountInGroup)
        {
            for (int i = enemyParent.transform.childCount - 1; i >= 0; i--)
            {
                enemyParent.transform.GetChild(i).SetParent(playerParent.transform);
            }
            InitializeCharacters();
            TurnExploreModeOn();
            BeginPlayerTurn();
            enemyParent.GetComponent<Enemy>().ClearSpawnedEnemy();
        }
        else
        {
            DialogueManager.Instance.StartDialogue(new string[] { "But looks like you already got so many people. I don't think you can accomodate more of us. Thanks for the offer. It means a lot. Goodbye" }, false, () =>
            {
                TurnManager.Instance.NPCRunAway();
            });
        }
    }

    void TurnExploreModeOn()
    {
        playerParent.GetComponent<Squad>().InitializeCharacters();
        TileManager.Instance.ClearTileOccupants();
        PostBattlePanel.SetActive(true);
        BeginPlayerTurn();
    }
    #endregion
}
