using UnityEngine;

public class SafeZoneTrigger : MonoBehaviour
{
    public GameObject gameWonPanel; // Drag your GameWonPanel here

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure your squad is tagged as "Player"
        {
            Debug.Log("Zone reached! Game won!");

            if (gameWonPanel != null)
            {
                gameWonPanel.SetActive(true);
            }

            // Disable movement
            SquadMover squadMover = other.GetComponent<SquadMover>();
            if (squadMover != null)
            {
                squadMover.enabled = false;
            }
        }
    }
}
