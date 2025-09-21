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

    [Header("Life time")]
    public bool HasLife = true;
    public float Lifetime = 120f;     // how long this event should live
    private float _timeRemaining;     // countdown timer

    void Update()
    {
        if (HasLife && _timeRemaining > 0f)
        {
            _timeRemaining -= Time.deltaTime;

            if (_timeRemaining <= 0f)
            {
                // life ended
                OnLifeEnded();
            }
        }
    }

    public void Instantiate()
    {
        if (HasLife)
        {
            _timeRemaining = Lifetime; // reset timer
        }

        // You can also randomize appearance here if needed:
        // Example: flip EventSprite horizontally
        if (EventSprite != null)
        {
            Vector3 scale = EventSprite.transform.localScale;
            scale.x = Random.value < 0.5f ? -1f : 1f;
            EventSprite.transform.localScale = scale;
        }
    }

    private void OnLifeEnded()
    {
        if (CanBeDestroyed)
        {
            Destroy(gameObject);
        }
        else
        {
            // Just deactivate if not destroyable
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure your squad is tagged as "Player"
        {
            var recent = other.GetComponent<EventTriggerData>()?.RecentEventTriggerData;
            if (recent == null || !ReferenceEquals(recent, this.gameObject))
            {
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

    public bool AllowRetreat = true;

    public int NPCLevel = 1;
}
