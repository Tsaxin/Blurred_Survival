using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CutSceneTrigger : MonoBehaviour
{
    public string CutsceneID;
    public GameObject EventAfterCutScene;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SquadMover.Instance.StopMovementOnEncounter();
            CutsceneManager.Instance.cutsceneID=CutsceneID;
            CutsceneManager.Instance.PlayCutsceneByID(EventAfterCutScene);
            Destroy(this.gameObject);
        }
    }
}
