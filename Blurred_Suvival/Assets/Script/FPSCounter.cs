using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI Counter;
    private float deltaTime = 0.0f;
    public bool ShowCounter = true;
    public GameObject Panel;

    void Start()
    {
        Application.targetFrameRate = 60;

        if (ShowCounter)
        {
            Panel.SetActive(ShowCounter);
        }
    }

    void Update()
    {
        if (!ShowCounter) return;
        // Smooth the FPS a little
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        
        // Calculate FPS
        float fps = 1.0f / deltaTime;
        
        // Display FPS rounded to integer
        Counter.text = Mathf.Ceil(fps).ToString() + " FPS";
    }
}
