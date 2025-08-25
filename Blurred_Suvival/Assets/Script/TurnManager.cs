using System.Collections;
using System.Collections.Generic;
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

        Debug.Log($"✅ Found {playerCharacters.Count} player characters and {enemyCharacters.Count} enemies.");
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
            //CoroutineRunner.Instance.StartCoroutine(BeginEnemyTurn());
        }
    }


    public void OnPlayerFinishedMove(CharacterController character)
    {
        if (!activePlayerCharacters.Contains(character)) return;

        currentCharacterIndex++;

        Debug.Log($"➡️ Player {currentCharacterIndex}/{activePlayerCharacters.Count} moved.");

        if (currentCharacterIndex >= activePlayerCharacters.Count)
        {
            CoroutineRunner.Instance.StartCoroutine(BeginEnemyTurn());
        }
    }


    [Header("Turn Timing")]
    public float delayBeforeEnemyTurn = 0.5f;   // 👈 New field

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
        foreach (var character in CC) {
            character.GetComponent<CharacterPassive>().TriggerTurnStartEffects();
        }
    }

}
