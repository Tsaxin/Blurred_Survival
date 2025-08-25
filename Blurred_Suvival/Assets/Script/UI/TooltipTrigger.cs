using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string itemName;
    private string description;

    public void Initialize(string name, string desc)
    {
        itemName = name;
        description = desc;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipUI.Instance.ShowTooltip(itemName, description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Instance.HideTooltip();
    }
}

