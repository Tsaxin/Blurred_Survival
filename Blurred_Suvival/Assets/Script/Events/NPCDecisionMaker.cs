using UnityEngine;

public class NPCDecisionMaker : MonoBehaviour
{
    [Header("References")]
    public Transform Squad;       // Player's squad
    public Transform EnemySquad;  // NPC's squad

    [Header("Anomaly Settings")]
    [Range(0f, 1f)]
    public float anomalyChance = 0.01f; // Base anomaly chance (1%)

    [Tooltip("Maximum random fluctuation in anomaly chance each decision (e.g. 0.2 = ±20%)")]
    [Range(0f, 1f)]
    public float chaosFactor = 0.2f;

    private float currentChaosMultiplier = 1f; // Tracks how chaos affects chance over time

    /// <summary>
    /// Makes a decision for the NPC:
    /// 0 = terrified (avoid combat)
    /// 1 = fight back
    /// </summary>
    public int MakeDecision()
    {
        float squadPower = CalculateSquadPower(Squad);
        float enemyPower = CalculateSquadPower(EnemySquad);

        if (squadPower <= 0f) return 0;

        float ratio = enemyPower / squadPower;
        int decision;

        if (ratio >= 1.5f)
            decision = 1; // fight back
        else
            decision = 0; // terrified

        // Apply chaos
        float adjustedChance = ApplyChaosToAnomaly();

        // --- anomaly injection ---
        if (Random.value < adjustedChance)
        {
            decision = decision == 1 ? 0 : 1; // flip decision
            Debug.Log($"⚠️ Anomaly triggered in combat! Decision reversed. (Chance: {adjustedChance:F3})");
        }

        UpdateChaosFactor();
        return decision;
    }

    /// <summary>
    /// Decides if NPCs should join the player's squad.
    /// Returns true if the NPC's highest level <= player's highest level.
    /// </summary>
    // public bool MakeJoinDecision()
    // {
    //     int playerHighest = GetHighestLevel(Squad);
    //     int npcHighest = GetHighestLevel(EnemySquad);

    //     bool decision = npcHighest <= playerHighest;

    //     // Apply chaos
    //     float adjustedChance = ApplyChaosToAnomaly();

    //     // --- anomaly injection ---
    //     if (Random.value < adjustedChance)
    //     {
    //         decision = !decision; // flip join outcome
    //         Debug.Log($"⚠️ Anomaly triggered in recruitment! Join decision reversed. (Chance: {adjustedChance:F3})");
    //     }

    //     UpdateChaosFactor();
    //     return decision;
    // }

    public bool MakeJoinDecision()
    {
        int playerHighest = GetHighestLevel(Squad);
        int npcHighest = GetHighestLevel(EnemySquad);

        // --- Base join chance ---
        // 80% base, modified slightly by level difference
        float levelDifference = playerHighest - npcHighest;
        float baseJoinChance = 0.8f; // 80% default success rate

        // ±5% per level difference (player stronger = higher chance)
        baseJoinChance += levelDifference * 0.05f;
        baseJoinChance = Mathf.Clamp01(baseJoinChance); // ensure within 0–1

        bool decision = Random.value < baseJoinChance;

        Debug.Log($"Recruitment decision: {(decision ? "✅ Success" : "❌ Fail")} | BaseChance={baseJoinChance:P0}");
        return decision;
    }

    // ===== Helper Functions =====

    private float CalculateSquadPower(Transform squad)
    {
        float total = 0f;
        foreach (Transform child in squad)
        {
            CharacterStats stats = child.GetComponent<CharacterStats>();
            if (stats != null)
                total += stats.Level; // Add more stats later if needed
        }
        return total;
    }

    private int GetHighestLevel(Transform squad)
    {
        int highest = 0;
        foreach (Transform child in squad)
        {
            CharacterStats stats = child.GetComponent<CharacterStats>();
            if (stats != null && stats.Level > highest)
                highest = stats.Level;
        }
        return highest;
    }

    /// <summary>
    /// Applies chaos variation to the base anomaly chance.
    /// </summary>
    private float ApplyChaosToAnomaly()
    {
        float chaosVariation = Random.Range(-chaosFactor, chaosFactor);
        float adjustedChance = Mathf.Clamp01(anomalyChance * (1f + chaosVariation) * currentChaosMultiplier);
        return adjustedChance;
    }

    /// <summary>
    /// Slowly shifts the chaos multiplier to simulate a dynamic world.
    /// </summary>
    private void UpdateChaosFactor()
    {
        // Chaos slightly fluctuates after each decision
        float chaosDrift = Random.Range(-0.05f, 0.05f);
        currentChaosMultiplier = Mathf.Clamp(currentChaosMultiplier + chaosDrift, 0.8f, 1.2f);
    }
}
