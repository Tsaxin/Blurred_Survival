using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingScreen;  // Assign your loading screen panel here
    public UnityEngine.UI.Slider progressBar; // Optional: slider for progress
    private Animator loadingAnimator;

    public static LoadingManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (loadingScreen != null)
            loadingAnimator = loadingScreen.GetComponent<Animator>();
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        // Reset progress bar
        if (progressBar != null)
            progressBar.value = 0f;

        // Reset animation if available
        if (loadingAnimator != null)
        {
            loadingAnimator.Rebind();       // reset animator state
            loadingAnimator.Update(0f);     // force it to refresh
        }

        // Show the loading UI
        loadingScreen.SetActive(true);

        // Start async loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        // Prevent auto-activation until we're ready
        operation.allowSceneActivation = false;

        // While loading
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Hide loading screen when done
        loadingScreen.SetActive(false);
    }
}
