using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleQueueManager : MonoBehaviour
{
    public static BattleQueueManager Instance;

    private Queue<BattleRequest> battleQueue = new Queue<BattleRequest>();
    private bool isBattleActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Adds a battle request to the queue.
    /// </summary>
    public void EnqueueBattle(bool isEvent, Event eventData, Region region, bool selfEncounter, GameObject battlefield = null)
    {
        if (isEvent)
        {
            // Event battle → nuke all randoms waiting
            Debug.Log("⚡ Event battle requested: clearing all queued random encounters.");
            ClearRandomEncountersFromQueue();
        }
        else
        {
            // If there's already an event queued, ignore this random battle
            foreach (var battle in battleQueue)
            {
                if (battle.IsEvent)
                {
                    Debug.Log("❌ Random encounter discarded because an event battle is queued.");
                    return;
                }
            }
        }

        battleQueue.Enqueue(new BattleRequest(isEvent, eventData, region, selfEncounter, battlefield));
        TryStartNextBattle();
    }

    /// <summary>
    /// Tries to start the next battle if none is active.
    /// </summary>
    private void TryStartNextBattle()
    {
        if (isBattleActive || battleQueue.Count == 0) return;

        BattleRequest request = battleQueue.Dequeue();
        StartBattle(request);
    }

    /// <summary>
    /// Starts a battle and marks it as active.
    /// </summary>
    private void StartBattle(BattleRequest request)
    {
        isBattleActive = true;

        Squad.Instance.InitiateBattleInternal(
            request.IsEvent,
            request.EventData,
            request.Region,
            request.SelfEncounter,
            request.BattleField
        );

        Debug.Log($"▶️ Battle started. Event: {request.IsEvent}");
    }

    /// <summary>
    /// Call this when a battle ends to allow the next queued battle to start.
    /// </summary>
    public void EndBattle()
    {
        isBattleActive = false;
        Debug.Log("🏁 Battle ended. Checking queue...");
        TryStartNextBattle();
    }

    /// <summary>
    /// Clears only random encounters from the queue (keeps event battles).
    /// </summary>
    private void ClearRandomEncountersFromQueue()
    {
        Queue<BattleRequest> tempQueue = new Queue<BattleRequest>();

        foreach (var battle in battleQueue)
        {
            if (battle.IsEvent)
            {
                tempQueue.Enqueue(battle);
            }
            else
            {
                Debug.Log("🗑️ Random encounter discarded.");
            }
        }

        battleQueue = tempQueue;
    }

    /// <summary>
    /// Represents a battle request.
    /// </summary>
    private struct BattleRequest
    {
        public bool IsEvent;
        public Event EventData;
        public Region Region;
        public bool SelfEncounter;
        public GameObject BattleField;

        public BattleRequest(bool isEvent, Event eventData, Region region, bool selfEncounter, GameObject battlefield)
        {
            IsEvent = isEvent;
            EventData = eventData;
            Region = region;
            SelfEncounter = selfEncounter;
            BattleField = battlefield;
        }
    }
}
