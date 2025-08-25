using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance;

    public GameObject damageTextPrefab;
    public Canvas worldCanvas; // Should be World Space or Screen Space - Camera

    public Color DamageColor = Color.red;
    public Color HealColor;
    public Color XPColor = Color.yellow;

    public Color EvadeColor;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Prevent duplicates
    }

    /// <summary>
    /// Show numeric damage popup (red).
    /// </summary>
    public void ShowDamage(Vector3 worldPosition, int amount)
    {
        ShowFloatingText(worldPosition, amount.ToString(), DamageColor);
    }
    
    public void ShowHeal(Vector3 worldPosition, int amount)
    {
        ShowFloatingText(worldPosition, amount.ToString(), HealColor);
    }

    public void ShowDamage(Vector3 worldPosition, string Damage, bool Evade)
    {
        Color color = DamageColor;
        if (Evade)
        {
            color = EvadeColor;
        }
        ShowFloatingText(worldPosition, Damage, color);
    }

    /// <summary>
    /// Show XP gain popup (yellow).
    /// </summary>
    public void ShowXP(Vector3 worldPosition, int amount)
    {
        ShowFloatingText(worldPosition, $"+{amount} XP", XPColor);
    }

    /// <summary>
    /// Spawn a floating text UI at world position.
    /// </summary>
    private void ShowFloatingText(Vector3 worldPosition, string text, Color color)
    {
        if (damageTextPrefab == null || worldCanvas == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject instance = Instantiate(damageTextPrefab, worldCanvas.transform);

        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, worldCanvas.worldCamera, out localPoint))
        {
            instance.GetComponent<RectTransform>().anchoredPosition = localPoint;
        }

        DamageText dt = instance.GetComponent<DamageText>();
        if (dt != null)
        {
            dt.SetText(text);
            dt.SetColor(color);
        }
    }
}
