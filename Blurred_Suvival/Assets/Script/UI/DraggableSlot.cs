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

        GameObject hovered = eventData.pointerCurrentRaycast.gameObject;

        if (hovered == null || hovered == gameObject)
        {
            ReturnToOriginalPosition();
            return;
        }

        // Handle DeleteRegion
        if (hovered.GetComponent<DeleteRegion>() != null)
        {
            if (index >= 0 && index < PlayerInventory.Instance.collectedItems.Count)
            {
                ItemData itemToRemove = PlayerInventory.Instance.collectedItems[index].data;
                PlayerInventory.Instance.RemoveItem(itemToRemove);
                Destroy(gameObject);
            }
            return;
        }

        // Handle ShortCutSlot
        ShortCutSlot slot = hovered.GetComponent<ShortCutSlot>();
        if (slot != null)
        {
            PlayerInventory.Instance?.DragAndDropSlot(slot,index,this);
            return;
        }

        // Default: return to original
        ReturnToOriginalPosition();
    }

    private void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.position = originalPosition;
    }

}
