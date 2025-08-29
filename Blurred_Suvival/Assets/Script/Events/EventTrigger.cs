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
    [TextArea]
    public string[] dialouge;
    public List<GameObject> Survivors;

    public List<GameObject> Vehicles;

    public List<GameObject> LootableObjects;

}
