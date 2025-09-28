using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    [Header("UI References")]
    public Image overlay;        // The overlay image (with CanvasGroup or Image)
    public GameObject panel;     // The panel behind the overlay

    [Header("Fade Settings")]
    public float fadeSpeed = 1f; // Speed at which alpha decreases

    void OnEnable()
    {
        if (overlay != null)
        {
            overlay.gameObject.SetActive(true);
            Color c = overlay.color;
            c.a = 1f; // start fully opaque
            overlay.color = c;
        }

        if (panel != null)
            panel.SetActive(true);

        StartCoroutine(FadeOutOverlay());
    }

    private IEnumerator FadeOutOverlay()
    {
        while (overlay != null && overlay.color.a > 0f)
        {
            Color c = overlay.color;
            c.a -= fadeSpeed * Time.deltaTime;
            overlay.color = c;
            yield return null; // wait until next frame
        }

        if (overlay != null)
            overlay.gameObject.SetActive(false);
    }

    public void GoToMainMenu()
    {
        SaveLoader.Instance.Save();
        MusicManager.Instance.PlayAmbientMusic();
        LoadingManager.Instance.LoadScene(0); // Loads the scene at index 0 (your main menu)
    }

    public void GoToMainMenuAfterGameOver()
    {
        MusicManager.Instance.PlayAmbientMusic();
        LoadingManager.Instance.LoadScene(0); // Loads the scene at index 0 (your main menu)
    }

    public void KeepPlaying()
    {
        panel.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Retry()
    {
        MusicManager.Instance.PlayAmbientMusic();
        LoadingManager.Instance.LoadScene(1);
    }
}
