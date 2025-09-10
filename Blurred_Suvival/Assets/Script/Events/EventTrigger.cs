using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTrigger : MonoBehaviour
{
    public SpriteRenderer EvenTypeIndicator, EventSprite;

    public bool EventCompleted = false;
    public bool CanBeDestroyed = true;
    public GameObject BattleField;
    public GameObject UnlockedEventTrigger;
    public string PlayCutSceneIdOnFinish;
    public Event newEvent;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure your squad is tagged as "Player"
        {
            var recent = other.GetComponent<EventTriggerData>()?.RecentEventTriggerData;
            if (recent == null || !ReferenceEquals(recent, this.gameObject))
            {
                Debug.Log("Still triggers");
                if (!EventCompleted)
                {
                    EventCompleted = true;
                    SquadMover.Instance.EnableEncounter(true, this, false, CanBeDestroyed, BattleField);
                }
                else
                {
                    SquadMover.Instance.EnableEncounter(false, this, false, CanBeDestroyed, BattleField);
                }

                if (UnlockedEventTrigger != null)
                {
                    UnlockedEventTrigger.SetActive(true);
                }

                CutsceneManager.Instance.cutsceneID = PlayCutSceneIdOnFinish;
                PlayCutSceneIdOnFinish = "0";

                other.GetComponent<EventTriggerData>().RecentEventTriggerData = this.gameObject;
            }
        }
    }
}

[System.Serializable]
public class Event
{
    public int SurvivorCount;
    [TextArea]
    public string[] dialouge;

    [HideInInspector]
    public Sprite Icon;

    public List<GameObject> Survivors;

    [HideInInspector]
    public List<GameObject> Vehicles;

    [HideInInspector]
    public List<GameObject> LootableObjects;

    public bool ShowChoiceButtonAtEndOfDialouge = true;

    public int DialougeChangeFrequency = 1;

    [Header("ShowChoiceButtonAtEndOfDialouge=False")]
    public ChoiceOutcome.ChoiceType PrimaryChoiceType;

    public bool AllowRetreat=true;
}
