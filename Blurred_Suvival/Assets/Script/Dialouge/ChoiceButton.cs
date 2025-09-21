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
        transform.localScale = Vector2.one;
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

        // --- Check if Runaway, Threaten, and Fight are ALL present ---
        bool hasRunaway = enabledFlags.Contains(ChoiceOutcome.ChoiceType.Runaway);
        bool hasThreaten = enabledFlags.Contains(ChoiceOutcome.ChoiceType.Threaten);
        bool hasFight = enabledFlags.Contains(ChoiceOutcome.ChoiceType.Fight);
        bool hasJoin = enabledFlags.Contains(ChoiceOutcome.ChoiceType.Join);
        
        // Find NPCDecisionMaker in scene
        NPCDecisionMaker npcDecision = GameObject.FindGameObjectWithTag("NPC Decision Maker")
                                                     ?.GetComponent<NPCDecisionMaker>();

        if (hasRunaway && hasThreaten && hasFight)
        {
            if (npcDecision != null)
            {
                int decision = npcDecision.MakeDecision();

                if (decision == 1)
                {
                    // Random between Fight and Threaten
                    ChoiceOutcome.ChoiceType[] fightOrThreaten = {
                    ChoiceOutcome.ChoiceType.Fight,
                    ChoiceOutcome.ChoiceType.Threaten
                };
                    return fightOrThreaten[UnityEngine.Random.Range(0, fightOrThreaten.Length)];
                }
                else
                {
                    // Pick randomly from all EXCEPT Fight & Threaten
                    List<ChoiceOutcome.ChoiceType> withoutFightThreaten =
                        new List<ChoiceOutcome.ChoiceType>(enabledFlags);

                    withoutFightThreaten.Remove(ChoiceOutcome.ChoiceType.Fight);
                    withoutFightThreaten.Remove(ChoiceOutcome.ChoiceType.Threaten);

                    return withoutFightThreaten[UnityEngine.Random.Range(0, withoutFightThreaten.Count)];
                }
            }
        }
        else if (hasJoin)
        {
            if (npcDecision != null)
            {
                if (npcDecision.MakeJoinDecision())
                {
                    return enabledFlags[UnityEngine.Random.Range(0, enabledFlags.Count)];
                }
                else {
                    List<ChoiceOutcome.ChoiceType> withoutJoin =
                        new List<ChoiceOutcome.ChoiceType>(enabledFlags);

                    withoutJoin.Remove(ChoiceOutcome.ChoiceType.Join);

                    return withoutJoin[UnityEngine.Random.Range(0, withoutJoin.Count)];
                }
            }
        }

        // --- Default random pick if condition not met ---
        return enabledFlags[UnityEngine.Random.Range(0, enabledFlags.Count)];
    }
}
