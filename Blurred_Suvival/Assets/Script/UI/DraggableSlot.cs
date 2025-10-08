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
        if (hovered != null)
        {// or however you track slots

            if (hovered.GetComponent<DeleteRegion>() != null)
            {
                if (index >= 0 && index < PlayerInventory.Instance.collectedItems.Count)
                {
                    ItemData itemToRemove = PlayerInventory.Instance.collectedItems[index].data;

                    // ✅ Directly call PlayerInventory
                    PlayerInventory.Instance.RemoveItem(itemToRemove);

                    // Destroy the UI slot
                    Destroy(gameObject);
                }
            }
            else if (hovered.GetComponent<ShortCutSlot>() != null)
            {
                ShortCutSlot slot = hovered.GetComponent<ShortCutSlot>();

                // If slot already has an item, you can swap or replace
                if (slot.AssignedItem != null)
                {
                    // Option 1: Swap items
                    ItemData itemData=slot.AssignedItem.data;
                    int itemQuantity = slot.AssignedItem.quantity;
                    slot.SetOnClick(PlayerInventory.Instance.collectedItems[index], TurnManager.Instance?.SelectedUnit);
                    
                    PlayerInventory.Instance.collectedItems.RemoveAt(index);
                    // Now, put the old item back in the inventory
                    PlayerInventory.Instance.AddItem(itemData,itemQuantity);
                    InventoryUIManager.Instance.RefreshInventory();
                }
                else
                {
                    // Assign item to empty shortcut
                    slot.SetOnClick(PlayerInventory.Instance.collectedItems[index], TurnManager.Instance?.SelectedUnit);

                    // Remove from inventory
                    PlayerInventory.Instance.collectedItems.RemoveAt(index);
                }

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
