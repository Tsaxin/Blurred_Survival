using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    public GameObject slotPrefab;
    public Transform slotParent;
    public PlayerInventory playerInventory;
    public GameObject UI;
    private List<GameObject> currentSlots = new List<GameObject>();

    public GameObject ToolTipPanel;

    public Canvas MainCanvas;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Avoid duplicate
            return;
        }

        Instance = this;

        RefreshInventory();
    }

    public void OpenInventory()
    {
        SFXManager.Instance?.PlayInventorySound();
        RefreshInventory();
        UI.gameObject.SetActive(true);
    }

    public void CloseInventory()
    {
        UI.gameObject.SetActive(false);
        ToolTipPanel.SetActive(false);
    }

    public void RefreshInventory()
    {
        foreach (var slot in currentSlots)
        {
            Destroy(slot);
        }
        currentSlots.Clear();

        foreach (ItemInstance item in playerInventory.collectedItems)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotParent);

            // Set quantity display
            Slot slotComponent = slotGO.GetComponent<Slot>();
            slotComponent.SetImage(item.data.itemIconIU);
            if (item.quantity > 1)
            {
                slotComponent.SetText(item.quantity.ToString());
            }
            else
            {
                slotComponent.QuantityHolder.SetActive(false);
            }

            // Tooltip
            TooltipTrigger trigger = slotGO.GetComponent<TooltipTrigger>();
            string name = item.data.itemName;
            string desc = TooltipUI.Instance.GenerateTooltipText(item);
            trigger.Initialize(name, desc);

            // Button click
            Button btn = slotGO.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                InventoryItemClickHandler.Instance.OnItemSlotClicked(item, TurnManager.Instance.SelectedUnit);
            });

            currentSlots.Add(slotGO);
        }
    }
    public GameObject GearPanel;
    public void ShowGear()
    {
        GearPanel.SetActive(true);
    }
}
