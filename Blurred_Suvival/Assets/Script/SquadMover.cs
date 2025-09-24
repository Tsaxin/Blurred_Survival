using System;
using System.Collections.Generic;
using UnityEngine;

public class SquadMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    [Header("Scene Switching")]
    public GameObject interactionObject; // assign your battle UI or encounter object
    public GameObject mapObject;         // assign your map object to disable it
    public GameObject BattleOverPanel;   // Battle panel over

    // Expose encounterChance for Region to set
    public float encounterChance = 0.15f;

    private float encounterTimer = 0.1f;
    public float CheckEncounterTime = 0.1f;
    public Region region;
    public Squad squad;

    public Transform EnemyHolder;

    public static SquadMover Instance;

    public GameObject CampButton;

    public GlobalTurnIndicator globalTurnIndicator;

    float InitialScale;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        InitialScale = transform.localScale.x;
        squad.SetSquadNumberText();
    }

    void Update()
    {
        // Movement input
        if (Input.GetMouseButtonDown(0))
        {
            if (UIBlocker.IsPointerOverUI())
                return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            targetPosition = mousePos;
            isMoving = true;
            encounterTimer = 0f; // Reset timer when player moves
        }

        if (isMoving)
        {
            if (targetPosition.x >= transform.position.x)
            {
                transform.localScale = new Vector2(InitialScale, InitialScale);
            }
            else
            {
                transform.localScale = new Vector2(-InitialScale, InitialScale);
            }
            GetComponent<Animator>()?.SetFloat("Moving",1f);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
                GetComponent<Animator>()?.SetFloat("Moving",0f);
                encounterTimer = 0f; // Reset timer when movement stops
            }

            // Encounter checking only if inside a region
            if (region != null)
            {
                encounterTimer += Time.deltaTime;
                if (encounterTimer >= CheckEncounterTime) // fixed interval
                {
                    encounterTimer = 0f;
                    float roll = UnityEngine.Random.value;
                    if (roll <= encounterChance)
                    {
                        EnableEncounter(false, null,false,false);
                    }
                }
            }
        }
    }

    public void EnableEncounter(bool IsEvent, EventTrigger eventTrigger,bool SelfEncounter,bool CanBeDestroyed,GameObject BattleField=null)
    {
        Event eventData = eventTrigger?.newEvent;
        if (CanBeDestroyed)
        {
            Destroy(eventTrigger.gameObject);
        }
        
        TriggerEncounterUI();
        if (BattleField != null)
        {
            squad.RequestBattle(IsEvent, eventData, region, SelfEncounter, BattleField);
        }
        else if (region != null)
        {
            squad.RequestBattle(IsEvent, eventData, region, SelfEncounter);
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public void StopMovementOnEncounter()
    {
        isMoving = false;
    }

    // Event to notify encounter trigger
    public delegate void EncounterAction();

    public void TriggerEncounterUI()
    {
        if (interactionObject != null)
            interactionObject.SetActive(true);

        if (mapObject != null)
            mapObject.SetActive(false);

        CoroutineRunner.Instance.StartCoroutine(CutsceneManager.Instance.Fade(0));   //Forcing fade out for cutscene manager

        StopMovementOnEncounter();
    }
    [Header("Post Battle Panel")]
    public GameObject PostBattlePanel;
    public void InitiateSelfEncounter()
    {
        EnableEncounter(false, null, true, false);
        PostBattlePanel.SetActive(true);
    }

    public void ExitEncounter()
    {
        globalTurnIndicator.GlobalTurnIndicatorState(false);
        RetreatHandler.CanRetreat = true;
        BattleQueueManager.Instance.EndBattle();
        ZombieSoundManager.Instance.StopZombieSound();
        MusicManager.Instance?.PlayAmbientMusic();

        if (TurnManager.Instance.SelectedUnit != null)
        {
            TurnManager.Instance.SelectedUnit.Deselect();
        }
        if (interactionObject != null)
            interactionObject.SetActive(false);

        if (mapObject != null)
            mapObject.SetActive(true);

        if (BattleOverPanel != null)
            BattleOverPanel.SetActive(false);

        EnemyHolder.GetComponent<Enemy>().DestroyAllChildren();
        BattleGroundManager.Instance.RemoveMap();
        CampButton.SetActive(true);
        CutsceneManager.Instance.PlayCutsceneByID();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Region"))
        {
            region = collision.GetComponent<Region>();
            encounterChance = region.EncounterChance;
        }
    }
}
