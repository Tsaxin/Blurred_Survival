using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float lifetime = 0.6f;      // total duration text stays alive
    public float moveDistance = 50f;   // pixels to move upwards
    public float popScale = 1.5f;      // maximum scale at peak
    public float speed = 3f;           // how fast it pops and settles

    [NonSerialized] public Action OnAnimationComplete;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetText(string value) => text.text = value;
    public void SetColor(Color c) => text.color = c;

    public void Activate(Vector2 startPosition)
    {
        rectTransform.anchoredPosition = startPosition;
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 1f;
        StartCoroutine(Animate());
    }

    private System.Collections.IEnumerator Animate()
    {
        float elapsed = 0f;
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 endPos = startPos + Vector3.up * moveDistance;

        while (elapsed < lifetime)
        {
            float t = elapsed / lifetime;

            // Move up
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);

            // Pop scale: pop up quickly and settle back
            float scaleT = Mathf.Sin(Mathf.Min(t * speed, 1f) * Mathf.PI); // Sin wave for pop
            float scale = Mathf.Lerp(1f, popScale, scaleT);
            rectTransform.localScale = Vector3.one * scale;

            // Fade out in last 30%
            if (t > 0.7f)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
        rectTransform.localScale = Vector3.one;
        canvasGroup.alpha = 0f;

        Disable();
    }

    private void Disable()
    {
        OnAnimationComplete?.Invoke(); // return to pool
    }
}
