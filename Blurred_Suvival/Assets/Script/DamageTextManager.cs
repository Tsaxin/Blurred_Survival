using UnityEngine;
using System.Collections.Generic;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance;

    public Canvas worldCanvas; // World Space or Screen Space - Camera
    public Color DamageColor = Color.red;
    public Color HealColor = Color.green;
    public Color XPColor = Color.yellow;
    public Color EvadeColor = Color.gray;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePoolFromChildren();
    }

    /// <summary>
    /// Load pool from existing child objects under this GameObject
    /// </summary>
    private void InitializePoolFromChildren()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            pool.Enqueue(child.gameObject);
        }

        if (pool.Count == 0)
            Debug.LogWarning("DamageTextManager: No child objects found for pool!");
    }

    private GameObject GetPooledObject()
    {
        if (pool.Count == 0)
        {
            Debug.LogWarning("DamageTextManager: Pool is empty, cannot get object!");
            return null;
        }

        GameObject obj = pool.Dequeue();

        // If it's active, recycle it
        if (obj.activeInHierarchy)
        {
            obj.SetActive(false);
        }

        return obj;
    }

    private void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    public void ShowDamage(Vector3 worldPosition, int amount)
    {
        ShowFloatingText(worldPosition, amount.ToString(), DamageColor);
    }

    public void ShowHeal(Vector3 worldPosition, int amount)
    {
        ShowFloatingText(worldPosition, amount.ToString(), HealColor);
    }
    public void ShowHeal(Vector3 worldPosition, string msg)
    {
        ShowFloatingText(worldPosition, msg, HealColor);
    }

    public void ShowDamage(Vector3 worldPosition, string damage, bool evade)
    {
        Color color = evade ? EvadeColor : DamageColor;
        ShowFloatingText(worldPosition, damage, color);
    }

    public void ShowXP(Vector3 worldPosition, int amount)
    {
        ShowFloatingText(worldPosition, $"+{amount} XP", XPColor);
    }
    public float offsetY = 2f;
    private void ShowFloatingText(Vector3 worldPosition, string text, Color color)
    {
        if (pool.Count == 0 || worldCanvas == null) return;

        GameObject instance = GetPooledObject();
        if (instance == null) return;

        instance.SetActive(true);

        // Apply world space Y offset first
        Vector3 offsetWorldPosition = worldPosition + Vector3.up * offsetY;

        // Convert world position (with offset) to canvas anchored position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(offsetWorldPosition);
        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                worldCanvas.worldCamera,
                out localPoint))
        {
            instance.GetComponent<RectTransform>().anchoredPosition = localPoint;
        }

        DamageText dt = instance.GetComponent<DamageText>();
        if (dt != null)
        {
            dt.SetText(text);
            dt.SetColor(color);

            // Pass the correct anchored position
            dt.Activate(localPoint + Vector2.up * 30f);

            dt.OnAnimationComplete = () => ReturnToPool(instance);
        }
    }


}
