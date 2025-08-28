using TMPro;
using UnityEngine;

public class ChoiceButton : MonoBehaviour
{
    public ChoiceOutcome choiceOutcome;
    public TextMeshProUGUI ResponseDialouge;

    void OnEnable()
    {
        ResponseDialouge.text = ChoiceHelper.GetRandomPlayerResponse(choiceOutcome);
    }

    public void OnFirstThreatClick()
    {
        // 1️⃣ Pick one of the enabled ChoiceTypes randomly
        ChoiceOutcome.ChoiceType selectedChoice = ChoiceHelper.GetRandomChoice(choiceOutcome);

        // 2️⃣ Call the function based on ChoiceType
        switch (selectedChoice)
        {
            case ChoiceOutcome.ChoiceType.Runaway:
                HandleRunaway();
                break;

            case ChoiceOutcome.ChoiceType.DropLoot:
                HandleDropLoot();
                break;

            case ChoiceOutcome.ChoiceType.Join:
                HandleJoin();
                break;

            case ChoiceOutcome.ChoiceType.Fight:
                HandleFight();
                break;

            default:
                Debug.LogWarning("No valid ChoiceType selected!");
                break;
        }
    }

    // 3️⃣ Separate functions for each ChoiceType
    private void HandleRunaway()
    {
        string enemyLine = ChoiceHelper.GetEnemyResponse(choiceOutcome, ChoiceOutcome.ChoiceType.Runaway);
        DialogueManager.Instance.StartDialogue(new string[] { enemyLine }, false);
    }

    private void HandleDropLoot()
    {
        string enemyLine = ChoiceHelper.GetEnemyResponse(choiceOutcome, ChoiceOutcome.ChoiceType.DropLoot);
        DialogueManager.Instance.StartDialogue(new string[] { enemyLine }, false);
    }

    private void HandleJoin()
    {
        string enemyLine = ChoiceHelper.GetEnemyResponse(choiceOutcome, ChoiceOutcome.ChoiceType.Join);
        DialogueManager.Instance.StartDialogue(new string[] { enemyLine }, false);
    }

    private void HandleFight()
    {
        string enemyLine = ChoiceHelper.GetEnemyResponse(choiceOutcome, ChoiceOutcome.ChoiceType.Fight);
        DialogueManager.Instance.StartDialogue(
            new string[] { enemyLine },
            false, // do not show choices again
            () =>
            {
                // Randomly select EnemyFirst or PlayerFirst
                ChoiceOutcome.TurnOrder firstToMove =
                    (Random.value < 0.5f) ? ChoiceOutcome.TurnOrder.EnemyFirst : ChoiceOutcome.TurnOrder.PlayerFirst;

                TurnManager.Instance.SetNPCAsEnemy(firstToMove);
            }
        );
    }
}
