using UnityEngine;

public class PauseMenuToggle: MonoBehaviour
{
    [Header("Assign your pause menu UI here")]
    public GameObject pauseMenu;

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false); // ensure it's hidden at start
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        HungerManager.Instance.IsPaused = isPaused;
        if (pauseMenu != null)
            pauseMenu.SetActive(isPaused);
    }
}
