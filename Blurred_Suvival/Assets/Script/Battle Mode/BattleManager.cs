using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameObject enemiesParent; // Assign your 'Enemies' GameObject in Inspector
    public Transform CharacterHolder;
    public int totalBattleXP = 0;

    private void OnEnable()
    {
        CalculateTotalXP();
    }

    void OnDisable()
    {
        BloodPool.Instance.ClearBloodSplashes();
    }

    public void CalculateTotalXP()
    {
        totalBattleXP = 0;
        foreach (Transform child in enemiesParent.transform)
        {
            CharacterStats stats = child.GetComponent<CharacterStats>();
            if (stats != null)
            {
                totalBattleXP += stats.xpPerKill;
            }
        }

        Debug.Log($"Total Battle XP calculated: {totalBattleXP}");
    }

    public void DistributeXPToAlivePlayers()
    {
        StartCoroutine(DistributeXP());
    }

    IEnumerator DistributeXP()
    {
        yield return new WaitForSeconds(0.5f);
        List<CharacterStats> alivePlayers = new List<CharacterStats>();

        foreach (Transform player in CharacterHolder.transform)
        {
            if (!player.GetComponent<CharacterStats>().isZombie && !player.GetComponent<CharacterStats>().IsDead)
            {
                alivePlayers.Add(player.GetComponent<CharacterStats>());
            }
        }

        if (alivePlayers.Count == 0)
        {
            Debug.Log("No alive players to distribute XP.");
        }
        else
        {
            int xpEach = totalBattleXP / alivePlayers.Count;

            foreach (var player in alivePlayers)
            {
                player.GainXP(xpEach);
                DamageTextManager.Instance.ShowXP(player.transform.position, xpEach);
            }

            Debug.Log($"Distributed {xpEach} XP to {alivePlayers.Count} alive players.");
        }
    }
}
