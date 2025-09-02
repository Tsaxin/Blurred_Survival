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
            if (other.GetComponent<EventTriggerData>()?.RecentEventTriggerData != this.gameObject)
            {
                if (!EventCompleted)
                {
                    EventCompleted = true;
                    SquadMover.Instance.EnableEncounter(true, this, false, CanBeDestroyed,BattleField);
                }
                else
                {
                    SquadMover.Instance.EnableEncounter(false, this, false, CanBeDestroyed,BattleField);
                }
                
                if (UnlockedEventTrigger != null)
                {
                    UnlockedEventTrigger.SetActive(true);
                }

                CutsceneManager.Instance.cutsceneID =PlayCutSceneIdOnFinish;
                PlayCutSceneIdOnFinish = "0";

                other.GetComponent<EventTriggerData>().RecentEventTriggerData = this.gameObject;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<EventTriggerData>().RecentEventTriggerData = null;
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

    [Header("ShowChoiceButtonAtEndOfDialouge=False")]
    public ChoiceOutcome.ChoiceType PrimaryChoiceType;

}
