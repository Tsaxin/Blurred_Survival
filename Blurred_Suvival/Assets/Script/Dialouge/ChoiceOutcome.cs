using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChoiceOutcome", menuName = "Dialogue/ChoiceOutcome")]
public class ChoiceOutcome : ScriptableObject
{
    [System.Flags]
    public enum ChoiceType
    {
        None = 0,
        Runaway = 1 << 0,
        DropLoot = 1 << 1,
        Join = 1 << 2,
        Fight = 1 << 3,
        Threaten = 1 << 4, // 16
        RejectJoin = 1 << 5,  // 32
        OfferTruce = 1 << 6  // 64
    }

    [System.Serializable]
    public class EnemyResponseSet
    {
        public ChoiceType choiceType;
        [TextArea] public List<string> Responses = new List<string>();
    }

    public List<EnemyResponseSet> EnemyResponses = new List<EnemyResponseSet>();

    public string GetEnemyResponse(ChoiceType choiceType)
    {
        foreach (var set in EnemyResponses)
        {
            if (set.choiceType == choiceType && set.Responses.Count > 0)
                return set.Responses[UnityEngine.Random.Range(0, set.Responses.Count)];
        }
        return "The enemy has no response...";
    }
}
