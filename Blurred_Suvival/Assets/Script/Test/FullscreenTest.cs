using UnityEngine;

public class FullscreenTest : MonoBehaviour
{
    void Start()
    {
        // Fullscreen mode
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        
        // Disable V-Sync
        QualitySettings.vSyncCount = 0;
        
        // Let Unity run uncapped
        Application.targetFrameRate = 1000; 

        Debug.Log("Fullscreen FPS test started");
    }

    void Update()
    {
        if (Time.frameCount % 60 == 0)
            Debug.Log("FPS: " + (1f / Time.unscaledDeltaTime));
    }
}
