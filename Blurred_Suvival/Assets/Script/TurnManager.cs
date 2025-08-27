using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GameObject playerParent; // 👈 Assign this in Inspector
    public GameObject enemyParent;  // 👈 Assign this in Inspector

    public List<CharacterController> playerCharacters;
    public List<ZombieAIBase> enemyCharacters;

    private int currentCharacterIndex = 0;
    private bool playerTurn = true;

    [Header("Turn Timing")]
    public float delayBetweenEnemyMoves = 0.2f; // Adjust in Inspector

    [Header("Post-Battle Settings")]
    public GameObject PostBattlePanel;

    private List<CharacterController> activePlayerCharacters = new List<CharacterController>();

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
            playerCharacters = new List<CharacterController>(playerParent.GetComponentsInChildren<CharacterController>());
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
        InitializeCharacters();

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

        // Now enemies move
        yield return BeginEnemyTurn();
    }

    IEnumerator HandleCamp()
    {
        yield return LoadCanvasGroup("Survivor Camp", "Survivors can do what they want!");
        BeginPlayerTurn();
    }
    public void BeginPlayerTurn()
    {
        playerTurn = true;
        currentCharacterIndex = 0;

        // Remove null or destroyed player references
        playerCharacters.RemoveAll(p => p == null);

        // Get only alive, existing players
        activePlayerCharacters = playerCharacters.FindAll(p => p != null && !p.GetComponent<CharacterStats>().IsDead);

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
                CoroutineRunner.Instance.StartCoroutine(BeginEnemyTurn());
            }
        }
    }



    [Header("Turn Timing")]
    public float delayBeforeEnemyTurn = 0.1f;   // 👈 New field

    IEnumerator BeginEnemyTurn()
    {
        playerTurn = false;
        Debug.Log("⏳ Waiting before enemy turn...");
        yield return new WaitForSeconds(delayBeforeEnemyTurn);

        Debug.Log("🔁 Enemy Turn Begins");

        enemyCharacters.RemoveAll(e => e == null);

        // Sort for consistent order
        enemyCharacters.Sort((a, b) =>
        {
            Vector2Int aIndex = a.GetTileIndices(a.currentTileData.transform);
            Vector2Int bIndex = b.GetTileIndices(b.currentTileData.transform);
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
        if (enemyCharacters.Contains(enemy))
        {
            enemyCharacters.Remove(enemy);
        }

        // Check if all enemies are dead
        if (enemyCharacters.Count == 0)
        {
            Debug.Log("🎉 All enemies defeated! Preparing to exit encounter...");

            // ✅ Distribute XP
            if (BattleManager != null)
            {
                BattleManager.DistributeXPToAlivePlayers();
            }
            else
            {
                Debug.LogWarning("⚠️ BattleManager not found when trying to distribute XP.");
            }

            // ✅ Show post battle UI
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
}
