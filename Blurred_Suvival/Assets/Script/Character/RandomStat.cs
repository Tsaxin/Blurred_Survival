using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomStat : MonoBehaviour
{
    public CharacterStats characterStats;

    public void GenerateRandomStat(int Level)
    {
        characterStats.Level=Level;
        // Give stat points equal to level - 1
        characterStats.statPoints = Level - 1;

        CharacterStatsUI StatManager = CharacterStatsUI.Instance;
        StatManager.targetStats = characterStats;

        for (int i = 0; i < Level - 1; i++) // use available statPoints
        {
            int roll = Random.Range(0, 5); // 0–4, since we have 5 functions

            switch (roll)
            {
                case 0:
                    StatManager.IncreaseAttack();
                    break;
                case 1:
                    StatManager.IncreaseHP();
                    break;
                case 2:
                    StatManager.IncreaseDefense();
                    break;
                case 3:
                    StatManager.IncreaseCrit();
                    break;
                case 4:
                    StatManager.IncreaseEvasion();
                    break;
            }
        }
    }

}
