using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string itemName;
    private string description;

    private Coroutine showTooltipCoroutine;

    public float ToolTipShowSecond = 1f;

    public void Initialize(string name, string desc)
    {
        itemName = name;
        description = desc;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Start delayed show
        showTooltipCoroutine = StartCoroutine(ShowTooltipWithDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelAndHide();
    }

    private IEnumerator ShowTooltipWithDelay()
    {
        yield return new WaitForSeconds(ToolTipShowSecond);
        TooltipUI.Instance?.ShowTooltip(itemName, description);
    }

    public void SetTriggerText(string Name, string Description)
    {
        Initialize(Name, Description);
    }

    private void CancelAndHide()
    {
        if (showTooltipCoroutine != null)
        {
            StopCoroutine(showTooltipCoroutine);
            showTooltipCoroutine = null;
        }
        TooltipUI.Instance?.HideTooltip();
    }

    private void OnDisable()
    {
        CancelAndHide();
    }

    private void OnDestroy()
    {
        CancelAndHide();
    }
}
