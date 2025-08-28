using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChoiceOutcome
{
    [System.Flags]
    public enum ChoiceType
    {
        None = 0,
        Runaway = 1 << 0,
        DropLoot = 1 << 1,
        Join = 1 << 2,
        Fight = 1 << 3
    }

    [Header("Choice Settings")]
    public ChoiceType choices; // multiple selection with checkboxes

    public enum TurnOrder { EnemyFirst, PlayerFirst }
    public TurnOrder firstToMove = TurnOrder.EnemyFirst;

    [Header("Player Responses")]
    [TextArea]
    public List<string> PlayerResponses = new List<string>(); // random pick from here

    [System.Serializable]
    public class EnemyResponseSet
    {
        public ChoiceType choiceType;
        [TextArea]
        public List<string> Responses = new List<string>(); // multiple options per choice
    }

    public List<EnemyResponseSet> EnemyResponses = new List<EnemyResponseSet>();

    public string GetRandomPlayerResponse()
    {
        if(PlayerResponses.Count == 0) return null;
        return PlayerResponses[UnityEngine.Random.Range(0, PlayerResponses.Count)];
    }
}

public static class ChoiceHelper
{
    public static ChoiceOutcome.ChoiceType GetRandomChoice(ChoiceOutcome outcome)
    {
        List<ChoiceOutcome.ChoiceType> available = new List<ChoiceOutcome.ChoiceType>();

        foreach (ChoiceOutcome.ChoiceType option in Enum.GetValues(typeof(ChoiceOutcome.ChoiceType)))
        {
            if (option != ChoiceOutcome.ChoiceType.None && outcome.choices.HasFlag(option))
                available.Add(option);
        }

        if (available.Count == 0) return ChoiceOutcome.ChoiceType.None;

        Debug.Log($"Available count: {available.Count}");
        return available[UnityEngine.Random.Range(0, available.Count)];
    }

    public static string GetRandomPlayerResponse(ChoiceOutcome outcome)
    {
        if (outcome.PlayerResponses.Count == 0) return "";
        return outcome.PlayerResponses[UnityEngine.Random.Range(0, outcome.PlayerResponses.Count)];
    }

    public static string GetEnemyResponse(ChoiceOutcome outcome, ChoiceOutcome.ChoiceType selectedChoice)
    {
        foreach (var set in outcome.EnemyResponses)
        {
            if (set.choiceType == selectedChoice && set.Responses.Count > 0)
            {
                return set.Responses[UnityEngine.Random.Range(0, set.Responses.Count)];
            }
        }

        return "The enemy has no response...";
    }
}

