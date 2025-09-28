using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    public TextMeshProUGUI tooltipText;
    public TextMeshProUGUI Name;

    public GameObject TooltipHolder;

    public GameObject Holder;
    public Vector2 padding = new Vector2(10f, 10f);

    private Canvas canvas;

    void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        HideTooltip();
    }
    private void FollowMouse()
    {
        Vector2 mousePos = Input.mousePosition;

        Vector2 HolderSize = Holder.GetComponent<RectTransform>().sizeDelta;

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Optional offset so tooltip is not exactly on top of the cursor
        Vector2 offset = new Vector2(40f, -40f); // 20px right, 20px below
        Vector2 targetPos = mousePos;

        if (targetPos.y - HolderSize.y +offset.y<= 0) // would cut off bottom
        {
            targetPos.x = mousePos.x + HolderSize.x / 2f + offset.x;
            targetPos.y =HolderSize.y / 2f - offset.y;
        }
        else
        {
            targetPos.y = mousePos.y - HolderSize.y / 2f + offset.y;
        }

        if ((targetPos.x + HolderSize.x / 2f) > screenWidth) // too far right
        {
            targetPos.x = mousePos.x - HolderSize.x / 2f; // move left
        }
        else if ((targetPos.x - HolderSize.x / 2f) < 0f)
        {
            targetPos.x = mousePos.x + HolderSize.x / 2f; // move left 
        }
        Holder.GetComponent<RectTransform>().position = targetPos;
    }
    public void ShowTooltip(string name, string description)
    {
        FollowMouse();
        TooltipHolder.SetActive(true);
        Name.text = name;
        tooltipText.text = description;
        tooltipText.ForceMeshUpdate();
    }

    public void HideTooltip()
    {
        if (TooltipHolder!=null) {
            TooltipHolder.SetActive(false);
        }
    }

    public string GenerateTooltipText(ItemInstance itemInstance)
    {
        var item = itemInstance.data;
        string desc = "";

        switch (item.itemType)
        {
            case ItemType.Weapon:
            case ItemType.Shoe:
            case ItemType.Vest:
            case ItemType.Trouser:
            case ItemType.Helmet:
                desc += BuildStatDescription((WeaponData)item);
                break;

            case ItemType.Consumable:
                var c = (ConsumableData)item;
                desc += $"<b>Heals:</b> {c.healthRestoreAmount} HP\n";
                break;
        }

        if (itemInstance.quantity > 1)
            desc += $"\n<b>Quantity:</b> {itemInstance.quantity}";

        return desc.TrimEnd();
    }

    private string BuildStatDescription(WeaponData w)
    {
        string desc = "";

        if (w.attackBoost != 0)
            desc += $"<b>Attack:</b> +{w.attackBoost}\n";
        if (w.attackCountBoost != 0)
            desc += $"<b>Attack Count:</b> +{w.attackCountBoost}\n";
        if (w.defenseBoost != 0)
            desc += $"<b>Defense:</b> +{w.defenseBoost}%\n";
        if (w.movementBoost != 0)
            desc += $"<b>Movement:</b> +{w.movementBoost} tiles\n";
        if (w.healthBoost != 0)
            desc += $"<b>Health:</b> +{w.healthBoost}\n";
        if (w.rangeBoost != 0)
            desc += $"<b>Range:</b> +{w.rangeBoost} tiles\n";
        if (w.criticalBoost != 0)
            desc += $"<b>Critical:</b> +{w.criticalBoost}%\n";
        if (w.evasionBoost != 0)
            desc += $"<b>Evasion:</b> +{w.evasionBoost}%\n";

        desc += $"\n\nNote: Click to equip. Equipping will cost one turn per survivor.";

        return desc;
    }
}
