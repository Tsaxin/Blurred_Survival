using UnityEngine;

public class NPCDecisionMaker : MonoBehaviour
{
    public Transform Squad, EnemySquad;

    [Range(0f, 1f)]
    public float anomalyChance = 0.01f; // 1% chance by default

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

        // --- anomaly injection ---
        if (Random.value < anomalyChance)
        {
            decision = decision == 1 ? 0 : 1; // flip decision
            Debug.Log("⚠️ Anomaly triggered! Decision reversed.");
        }

        return decision;
    }

    /// <summary>
    /// Decides if NPCs should join the player's squad.
    /// Returns true if the NPC's highest level <= player's highest level.
    /// </summary>
    public bool MakeJoinDecision()
    {
        int playerHighest = GetHighestLevel(Squad);
        int npcHighest = GetHighestLevel(EnemySquad);

        return npcHighest <= playerHighest;
    }

    private float CalculateSquadPower(Transform squad)
    {
        float total = 0f;
        foreach (Transform child in squad)
        {
            CharacterStats stats = child.GetComponent<CharacterStats>();
            if (stats != null)
            {
                total += stats.Level; // Or more complex formula
            }
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
            {
                highest = stats.Level;
            }
        }
        return highest;
    }
}
