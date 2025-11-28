using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShortCutSlot : MonoBehaviour
{
    [Tooltip("Stable ID for this shortcut slot (set in inspector). Used for save/load.")]
    public int SlotIndex = 0;

    public Image SpriteIcon;
    public TextMeshProUGUI Quantity;

    public GameObject QuantityHolder;

    // Holds the currently assigned item
    public ItemInstance AssignedItem { get; private set; }

    public void SetDetail(ItemInstance item = null)
    {
        if (item != null)
        {
            AssignedItem = item; // store the reference
        }
        SpriteIcon.sprite = item.data.itemIconIU;
        SpriteIcon.gameObject.SetActive(true);
        Quantity.text = item.quantity.ToString();
        QuantityHolder.gameObject.SetActive(StackableItem.IsStackable(item.data.itemType));

        gameObject.AddComponent<TooltipTrigger>();
        TooltipTrigger trigger =GetComponent<TooltipTrigger>();
        trigger.ToolTipShowSecond = 0;
        string name = item.data.itemName;
        string desc = TooltipUI.Instance.GenerateTooltipText(item);
        trigger.Initialize(name, desc);
    }

    public void SetOnClick(ItemInstance clickedItem, CharacterController selectedUnit)
    {
        SetDetail(clickedItem);

        AssignedItem = clickedItem; // make sure this is stored

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        CoroutineRunner.Instance?.StartCoroutine(AssignButtonNextFrame(btn, selectedUnit));
    }

    private IEnumerator AssignButtonNextFrame(Button btn, CharacterController selectedUnit)
    {
        yield return null; // ✅ wait 1 frame to ensure UI is initialized

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
        Destroy(GetComponent<TooltipTrigger>());
    }
}
