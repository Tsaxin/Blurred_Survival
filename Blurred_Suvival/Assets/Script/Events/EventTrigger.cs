using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTrigger : MonoBehaviour
{
    public Event newEvent;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure your squad is tagged as "Player"
        {
            SquadMover.Instance.EnableEncounter(true,this,false);
        }
    }
}

[System.Serializable]
public class Event
{
    public int SurvivorCount;
    [TextArea]
    public string[] dialouge;

    public Sprite Icon;
    [HideInInspector]
    public List<GameObject> Survivors;

    [HideInInspector]
    public List<GameObject> Vehicles;

    [HideInInspector]
    public List<GameObject> LootableObjects;

}
