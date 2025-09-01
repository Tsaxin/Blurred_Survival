using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChoiceButton : MonoBehaviour
{
    [Header("References")]
    public ChoiceOutcome outcomeData;   // Central responses
    public ChoiceOutcome.ChoiceType choiceType; // This button’s unique action
    public TextMeshProUGUI responseText;

    [Header("Player Responses")]
    [TextArea]
    public List<string> PlayerResponses = new List<string>();

    void OnEnable()
    {
        if (responseText != null)
            responseText.text = PlayerResponses[UnityEngine.Random.Range(0, PlayerResponses.Count)];
    }

    public void OnClick()
    {
        // ✅ If multiple ChoiceTypes are set, pick one at random
        ChoiceOutcome.ChoiceType selectedChoice = GetRandomChoiceFromFlags(choiceType);

        // Get enemy line for the selected choice
        string enemyLine = outcomeData.GetEnemyResponse(selectedChoice);
        Debug.Log(selectedChoice);

        switch (selectedChoice)
        {
            case ChoiceOutcome.ChoiceType.Runaway:
                DialogueManager.Instance.StartDialogue(new string[] { enemyLine }, false, () =>
                {
                    TurnManager.Instance.NPCRunAway();
                });
                break;

            case ChoiceOutcome.ChoiceType.DropLoot:
                DialogueManager.Instance.StartDialogue(new string[] { enemyLine }, false, () =>
                {
                    TurnManager.Instance.NPCDropLootAndRunAway();
                });
                break;

            case ChoiceOutcome.ChoiceType.Join:
                DialogueManager.Instance.StartDialogue(new string[] { enemyLine }, false, () =>
                {
                    TurnManager.Instance.NPCJoin();
                });
                break;

            case ChoiceOutcome.ChoiceType.Fight:
                DialogueManager.Instance.StartDialogue(new string[] { enemyLine }, false, () =>
                {
                    TurnManager.Instance.SetNPCAsEnemy();
                });
                break;
            case ChoiceOutcome.ChoiceType.Threaten:
                DialogueManager.Instance.StartDialogue(new string[] { enemyLine }, false, () =>
                {
                    TurnManager.Instance.SetNPCAsEnemy();
                });
                break;
            case ChoiceOutcome.ChoiceType.RejectJoin:
                DialogueManager.Instance.StartDialogue(new string[] { enemyLine }, false, () =>
                {
                    TurnManager.Instance.NPCRunAway();
                });
                break;
            case ChoiceOutcome.ChoiceType.OfferTruce:
                DialogueManager.Instance.StartDialogue(new string[] { enemyLine }, false, () =>
                {
                    TurnManager.Instance.NPCRunAway();
                });
                break;
        }
    }

    // ✅ Helper to pick a random enabled flag from a Flags enum
    private ChoiceOutcome.ChoiceType GetRandomChoiceFromFlags(ChoiceOutcome.ChoiceType flags)
    {
        List<ChoiceOutcome.ChoiceType> enabledFlags = new List<ChoiceOutcome.ChoiceType>();

        foreach (ChoiceOutcome.ChoiceType value in Enum.GetValues(typeof(ChoiceOutcome.ChoiceType)))
        {
            if (value == ChoiceOutcome.ChoiceType.None) continue;

            if (flags.HasFlag(value))
                enabledFlags.Add(value);
        }

        if (enabledFlags.Count == 0)
            return ChoiceOutcome.ChoiceType.None;

        return enabledFlags[UnityEngine.Random.Range(0, enabledFlags.Count)];
    }

}
