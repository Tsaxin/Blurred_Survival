using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTrigger : MonoBehaviour
{
    public SpriteRenderer EvenTypeIndicator, EventSprite;

    public bool EventCompleted = false;
    public bool CanBeDestroyed = true;

    public GameObject UnlockedEventTrigger;
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
                    SquadMover.Instance.EnableEncounter(true, this, false, CanBeDestroyed);
                }
                else
                {
                    SquadMover.Instance.EnableEncounter(false, this, false, CanBeDestroyed);
                }
                
                if (UnlockedEventTrigger != null)
                {
                    UnlockedEventTrigger.SetActive(true);
                }

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
