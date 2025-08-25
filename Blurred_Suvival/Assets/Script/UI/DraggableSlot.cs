using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;
    private Canvas canvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = InventoryUIManager.Instance.MainCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform); // bring to front
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        GameObject hovered = eventData.pointerEnter;
        if (hovered != null && hovered.GetComponent<DeleteRegion>() != null)
        {
            // Call InventoryUIManager to remove the item
            InventoryUIManager.Instance.DeleteWeaponSlot(gameObject);
        }
        else
        {
            // Return to original position
            transform.SetParent(originalParent);
            rectTransform.position = originalPosition;
        }
    }
}
