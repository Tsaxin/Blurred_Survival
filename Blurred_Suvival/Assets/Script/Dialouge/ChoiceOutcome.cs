using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChoiceOutcome", menuName = "Dialogue/ChoiceOutcome")]
public class ChoiceOutcome : ScriptableObject
{
    // --- Flags enum (bitmask) ---
    [System.Flags]
    public enum ChoiceType
    {
        None = 0,
        Runaway = 1 << 0,
        DropLoot = 1 << 1,
        Join = 1 << 2,
        Fight = 1 << 3,
        Threaten = 1 << 4,
        RejectJoin = 1 << 5,
        OfferTruce = 1 << 6
    }

    [System.Serializable]
    public class EnemyResponseSet
    {
        // Use int-backed property to safely serialize in Unity
        [SerializeField] private int choiceTypeValue = 0;

        public ChoiceType choiceType
        {
            get => (ChoiceType)choiceTypeValue;
            set => choiceTypeValue = (int)value;
        }

        [TextArea]
        public List<string> Responses = new List<string>();
    }

    public List<EnemyResponseSet> EnemyResponses = new List<EnemyResponseSet>();

    /// <summary>
    /// Returns a random enemy response for the given choice type
    /// </summary>
    public string GetEnemyResponse(ChoiceType choiceType)
    {
        foreach (var set in EnemyResponses)
        {
            // Check if this set contains the requested flag
            if (set.Responses.Count > 0 && (set.choiceType & choiceType) != 0)
            {
                return set.Responses[UnityEngine.Random.Range(0, set.Responses.Count)];
            }
        }

        return "The enemy has no response...";
    }
}
