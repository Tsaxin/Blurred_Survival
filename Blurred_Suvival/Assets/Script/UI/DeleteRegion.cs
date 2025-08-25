using UnityEngine;
using UnityEngine.EventSystems;

public class DeleteRegion : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // This will be handled in OnEndDrag of the slot.
        Debug.Log("Dropped");
    }
}
