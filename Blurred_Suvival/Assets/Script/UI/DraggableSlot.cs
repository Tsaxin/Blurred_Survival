using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;
    private Canvas canvas;
    int index;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = InventoryUIManager.Instance.MainCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        index = transform.GetSiblingIndex();
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
        {// or however you track slots
            if (index >= 0 && index < PlayerInventory.Instance.collectedItems.Count)
            {
                ItemData itemToRemove = PlayerInventory.Instance.collectedItems[index].data;

                // ✅ Directly call PlayerInventory
                PlayerInventory.Instance.RemoveItem(itemToRemove);

                // Destroy the UI slot
                Destroy(gameObject);
            }
        }
        else
        {
            // Return to original position
            transform.SetParent(originalParent);
            rectTransform.position = originalPosition;
        }
    }

}
