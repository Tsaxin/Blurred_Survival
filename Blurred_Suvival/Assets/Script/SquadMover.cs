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

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
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
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
                encounterTimer = 0f; // Reset timer when movement stops
            }

            // Encounter checking only if inside a region
            if (region != null)
            {
                encounterTimer += Time.deltaTime;
                if (encounterTimer >= CheckEncounterTime) // fixed interval
                {
                    encounterTimer = 0f;
                    float roll = Random.value;
                    if (roll <= encounterChance)
                    {
                        TriggerEncounterUI();
                    }
                }
            }
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
        squad.region = region;

        if (interactionObject != null)
            interactionObject.SetActive(true);

        if (mapObject != null)
            mapObject.SetActive(false);

        StopMovementOnEncounter();
    }

    public void InitiateSelfEncounter()
    {
        squad.SelfEncounter = true;
        TriggerEncounterUI();
    }

    public void ExitEncounter()
    {
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
