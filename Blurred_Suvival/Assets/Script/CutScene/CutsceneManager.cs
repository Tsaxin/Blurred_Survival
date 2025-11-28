using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class CutsceneManager : MonoBehaviour
{
    [Header("References")]
    public GameObject CutScenePanel;
    public GameObject MapObject;
    public CutsceneData cutsceneDatabase;
    public Image displayImage;
    public TextMeshProUGUI displayText;
    public Image fadeOverlay;

    [Header("Settings")]
    public float fadeDuration = 1f;
    public float imageDuration = 2f;
    public float moveDistance = 50f; // pixels to drift sideways

    private Vector3 originalPosition;
    private bool isSkipping = false;

    public static CutsceneManager Instance;

    public GameObject EventAfterCutScene;

    public string cutsceneID;
    public GameObject GameWinObject;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        originalPosition = displayImage.rectTransform.localPosition;
    }

    /// <summary>Call this from your UI Button (OnClick)</summary>
    public void RequestSkip()
    {
        if (isSkipping) return;

        isSkipping = true;
        // Stop EVERYTHING in this MonoBehaviour to avoid race conditions/flicker.
        StopAllCoroutines();
        // Start a clean, dedicated skip sequence.
        StartCoroutine(SkipSequence());
    }

    //This function is also called by battlemanager on 
    public void PlayCutsceneByID(GameObject EventAfterCutScene = null)
    {
        if (cutsceneID == "0")
        {
            return;     //meaning no cutscene after the fight
        }
        else if (cutsceneID == "Game won")
        {
            EventAfterCutScene = GameWinObject;
        }
        if (EventAfterCutScene != null)
        {
            this.EventAfterCutScene = EventAfterCutScene;
        }
        
        Cutscene cutscene = cutsceneDatabase.GetCutsceneByID(cutsceneID);
        if (cutscene != null)
            StartCoroutine(PlayCutscene(cutscene.entries));
        else
            Debug.LogWarning("Cutscene ID not found: " + cutsceneID);
    }

    private IEnumerator PlayCutscene(List<CutsceneEntry> entries)
    {
        cutsceneID = "0";
        fadeOverlay.gameObject.SetActive(true);
        Color alpha = fadeOverlay.color;
        alpha.a = 1f;
        fadeOverlay.color = alpha;

        for (int i = 0; i < entries.Count; i++)
        {
            // Fade to black first (no skip checks needed; skip uses StopAllCoroutines)
            yield return StartCoroutine(Fade(1));

            MapObject.SetActive(false);
            CutScenePanel.SetActive(true);

            // Set image and text
            displayImage.sprite = entries[i].image;
            displayText.text = entries[i].text;
            displayImage.rectTransform.localPosition = originalPosition;

            // Fade IN from black while moving right (continuous)
            float movementElapsed = 0f;
            float fadeTime = 0f;
            while (fadeTime < fadeDuration || movementElapsed < imageDuration)
            {
                float dt = Time.deltaTime;

                if (movementElapsed < imageDuration)
                {
                    movementElapsed += dt;
                    float tMove = Mathf.Clamp01(movementElapsed / imageDuration);
                    displayImage.rectTransform.localPosition =
                        originalPosition + new Vector3(moveDistance * tMove, 0, 0);
                }

                if (fadeTime < fadeDuration)
                {
                    fadeTime += dt;
                    float tFade = Mathf.Clamp01(fadeTime / fadeDuration);
                    Color c = fadeOverlay.color;
                    c.a = Mathf.Lerp(1f, 0f, tFade);
                    fadeOverlay.color = c;
                }

                yield return null;
            }

            // Fade OUT to black while a tiny drift continues
            float fadeToBlackTime = 0f;
            float moveDuringFade = 0f;
            while (fadeToBlackTime < fadeDuration)
            {
                float dt = Time.deltaTime;

                moveDuringFade += dt;
                float tMove = Mathf.Clamp01(moveDuringFade / fadeDuration);
                displayImage.rectTransform.localPosition += new Vector3(moveDistance * 0.1f * tMove, 0, 0);

                fadeToBlackTime += dt;
                float tFade = Mathf.Clamp01(fadeToBlackTime / fadeDuration);
                Color c = fadeOverlay.color;
                c.a = Mathf.Lerp(0f, 1f, tFade);
                fadeOverlay.color = c;

                yield return null;
            }

            // Snap only after fully black
            displayImage.rectTransform.localPosition = originalPosition;
        }

        // Finished normally
        OnDialougeEnd();

    }

    /// <summary>
    /// Clean skip: we’ve already stopped all coroutines.
    /// Now do a single, smooth sequence: fade to black -> end -> fade out.
    /// </summary>
    private IEnumerator SkipSequence()
    {
        // Fade to black smoothly
        yield return StartCoroutine(Fade(1));

        // Make sure the image is reset behind the black screen
        displayImage.rectTransform.localPosition = originalPosition;

        // End cutscene UI state
        OnDialougeEnd();

        isSkipping = false;
    }

    public IEnumerator Fade(float targetAlpha)
    {
        Color color = fadeOverlay.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        fadeOverlay.color = color;
    }

    private void OnDialougeEnd()
    {
        if (EventAfterCutScene != null)
        {
            EventAfterCutScene.SetActive(true);
        }
        else
        {
            StartCoroutine(Fade(0));
        }
        MapObject.SetActive(true);
        CutScenePanel.SetActive(false);
    }
}
