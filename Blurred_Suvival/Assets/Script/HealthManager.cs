using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public float baseDecayRate = 1f; // health per minute when idle
    public float movementDecayMultiplier = 2f;

    public Slider healthBarSlider;
    public GameObject gameOverPanel; // ← Drag your Game Over UI panel here

    private SquadMover squadMover;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        squadMover = GetComponent<SquadMover>();

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // Hide panel at start
        }
    }

    void Update()
    {
        if (isDead) return;

        float decayRate = baseDecayRate;

        if (squadMover != null && squadMover.IsMoving())
        {
            decayRate *= movementDecayMultiplier;
        }

        currentHealth -= decayRate * Time.deltaTime / 60f; // per minute
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        if (currentHealth <= 0f)
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        isDead = true;

        if (squadMover != null)
        {
            squadMover.enabled = false;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log("Game Over");
    }

    // Optional: Retry button can call this from the UI
    public void Retry()
    {
        // Reload scene or reset health and restart
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
