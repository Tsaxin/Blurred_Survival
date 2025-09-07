using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public Sprite EventBackground, EventSprite;

    public MasterEvent masterEvent;

    public Transform EventHolder;

    public float EventGeneratorCD;
    public GameObject EventTemplate;
    public int MaxEvent;

    [Header("Spawn Area")]
    public float MaxY = 3.6f;
    public float MinY = -4.5f, MaxX = 8.46f, MinX = -8.52f;
    public static EventManager Instance;

    float timer;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Update()
    {
        // Count down the timer
        timer -= Time.deltaTime;

        // When timer hits 0, check if we need to spawn
        if (timer <= 0f)
        {
            timer = EventGeneratorCD; // reset timer

            // Check if we have fewer events than max allowed
            if (EventHolder.childCount < MaxEvent)
            {
                GenerateEvent();
            }
        }
    }

    void GenerateEvent()
    {
        float randX = Random.Range(MinX, MaxX);
        float randY = Random.Range(MinY, MaxY);
        Vector3 spawnPos = new Vector3(randX, randY, 0f);

        // Instantiate event prefab as child of EventHolder
        GameObject newEvent = Instantiate(EventTemplate, spawnPos, Quaternion.identity, EventHolder);

        newEvent.GetComponent<EventTrigger>().newEvent=masterEvent.GetRandomEvent(newEvent.GetComponent<EventTrigger>().EventSprite);
    }
}
