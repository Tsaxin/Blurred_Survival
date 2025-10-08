using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShortCutSlot : MonoBehaviour
{
    public Image SpriteIcon;
    public TextMeshProUGUI Quantity;

    public GameObject QuantityHolder;

    // Holds the currently assigned item
    public ItemInstance AssignedItem { get; private set; }

    public void SetDetail(ItemInstance item)
    {
        AssignedItem = item; // store the reference
        SpriteIcon.sprite = item.data.itemIconIU;
        SpriteIcon.gameObject.SetActive(true);
        Quantity.text = item.quantity.ToString();
        QuantityHolder.gameObject.SetActive(true);
    }

    public void SetOnClick(ItemInstance clickedItem, CharacterController selectedUnit)
    {
        SetDetail(clickedItem);
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            InventoryItemClickHandler.Instance.OnItemSlotClicked(AssignedItem, selectedUnit, true, this);
        });
    }

    public void OnReset()
    {
        AssignedItem = null;
        GetComponent<Button>().onClick.RemoveAllListeners();
        QuantityHolder.gameObject.SetActive(false);
        SpriteIcon.gameObject.SetActive(false);
    }
}
