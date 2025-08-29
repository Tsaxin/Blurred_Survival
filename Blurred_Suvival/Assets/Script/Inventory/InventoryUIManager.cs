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

        Debug.Log("Function called");

        foreach (ItemInstance item in playerInventory.collectedItems)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotParent);
            Debug.Log("Loop called");

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
            string desc = GenerateTooltipText(item);
            trigger.Initialize(name, desc);

            // Button click
            Button btn = slotGO.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                Debug.Log($"Clicked on {item.data.itemName}");
                OnItemSlotClicked(item);
            });

            currentSlots.Add(slotGO);
        }
    }

    private void OnItemSlotClicked(ItemInstance clickedItem)
    {
        var selectedCharacter = TurnManager.Instance.SelectedUnit;
        if (selectedCharacter == null)
        {
            Debug.LogWarning("No character selected!");
            return;
        }

        switch (clickedItem.data.itemType)
        {
            case ItemType.Weapon:
                var gearEquipper = selectedCharacter.GetComponent<GearEquipper>();
                if (gearEquipper == null)
                {
                    Debug.LogWarning("Selected character has no WeaponEquipper.");
                    return;
                }

                // Remove one weapon instance
                playerInventory.collectedItems.Remove(clickedItem);

                if (gearEquipper.equippedWeapon != null)
                {
                    playerInventory.collectedItems.Add(new ItemInstance(gearEquipper.equippedWeapon));
                }

                gearEquipper.EquipWeapon((WeaponData)clickedItem.data);
                PlayerInventory.Instance.EnqueueCollectedText($"{clickedItem.data.itemName} Equipped!", PlayerInventory.FloatingTextType.Heal);
                break;

            case ItemType.Consumable:
                var consumableData = (ConsumableData)clickedItem.data;
                var characterStats = selectedCharacter.GetComponent<CharacterStats>();
                if (characterStats == null)
                {
                    Debug.LogWarning("Selected character has no CharacterStats.");
                    return;
                }

                // Heal the character
                characterStats.Heal(consumableData.healthRestoreAmount);

                // Reduce quantity or remove item
                clickedItem.quantity--;
                if (clickedItem.quantity <= 0)
                {
                    playerInventory.collectedItems.Remove(clickedItem);
                }
                break;

                // Add more item type cases here
        }

        RefreshInventory();
        selectedCharacter.ShowAvailableMoveTiles();
        TurnManager.Instance.SelectedUnit.FinishedTurn();
        CloseInventory();
    }

    public string GenerateTooltipText(ItemInstance itemInstance)
    {
        var item = itemInstance.data;
        string desc = "";

        switch (item.itemType)
        {
            case ItemType.Weapon:
                var w = (WeaponData)item;
                if (w.attackBoost != 0)
                    desc += $"<b>Attack:</b> +{w.attackBoost}\n";
                // (Add other weapon stats similarly)
                desc += $"<b>Type:</b> Weapon";
                break;

            case ItemType.Consumable:
                var c = (ConsumableData)item;
                desc += $"<b>Heals:</b> {c.healthRestoreAmount} HP\n";
                desc += $"<b>Type:</b> Consumable";
                break;
        }

        if (itemInstance.quantity > 1)
            desc += $"\n<b>Quantity:</b> {itemInstance.quantity}";

        return desc.TrimEnd();
    }

    public GameObject GearPanel;
    public void ShowGear()
    {
        GearPanel.SetActive(true);
    }
}
