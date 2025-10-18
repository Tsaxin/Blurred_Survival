using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftManager : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory;
    public LootMaster lootMaster;
    public GameObject CraftPanel, StatPanel, InventoryPanel, GearPanel;
    public GameObject SlotParent, CraftSlot, CraftButton;

    [Header("Inventory Lookup")]
    public Dictionary<ItemData, int> Inventory = new Dictionary<ItemData, int>();
    public List<LootEntry> allCraftableItems;

    [Header("Selected State")]
    public Slot SelectedSlot;
    public ItemData SelectedItem;

    public static CraftManager Instance;

    [Header("Shortcut Slot")]
    public Transform ShortcutSlotParent;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        allCraftableItems = new List<LootEntry>(lootMaster.GetAllCraftableLoot());
    }

    #region Panel Open/Close
    public void OpenCraftPanel()
    {
        CraftPanel.SetActive(true);
        if (StatPanel != null) StatPanel.SetActive(false);
        if (InventoryPanel != null) InventoryPanel.SetActive(false);
        if (GearPanel != null) GearPanel.SetActive(false);

        LoadCraftableItems();
    }

    public void CloseCraftPanel()
    {
        SelectedItem = null;
        if (SelectedSlot != null)
        {
            SelectedSlot.SetSelectedImage(false);
            SelectedSlot = null;
        }
        CraftButton.SetActive(false);
        CraftPanel.SetActive(false);
    }

    #endregion

    #region Load & Generate Slots
    public void LoadCraftableItems()
    {
        // Clear old slots
        foreach (Transform child in SlotParent.transform)
            Destroy(child.gameObject);

        // Populate inventory dictionary
        PopulateInventoryDictionary();

        // Keep track if previously selected item is still fully craftable
        bool prevItemStillCraftable = false;

        foreach (LootEntry lootEntry in allCraftableItems)
        {
            CraftableItem craftItem = lootEntry.loot.GetComponent<CraftableItem>();
            if (craftItem == null) continue;

            bool hasAllIngredients = true;
            bool hasAnyIngredient = false;

            foreach (CraftRequirement req in craftItem.requirements)
            {
                Inventory.TryGetValue(req.item, out int owned);

                if (owned > 0) hasAnyIngredient = true;
                if (owned < req.quantity) hasAllIngredients = false;
            }

            // Generate slot
            if (hasAllIngredients)
            {
                GenerateSlot(craftItem, true);

                // Check if this is the previously selected item
                if (SelectedItem != null && SelectedItem == craftItem.GetItemData())
                    prevItemStillCraftable = true;
            }
            else if (hasAnyIngredient)
                GenerateSlot(craftItem, false);
        }

        // Update selection based on full craftability
        if (!prevItemStillCraftable)
        {
            SelectedItem = null;
            if (SelectedSlot != null)
            {
                SelectedSlot.SetSelectedImage(false);
                SelectedSlot = null;
            }
            CraftButton.SetActive(false);
        }
    }

    void PopulateInventoryDictionary()
    {
        // Step 1: Clear
        Inventory.Clear();

        // Step 2: Add items from PlayerInventory
        foreach (ItemInstance item in playerInventory.collectedItems)
        {
            if (item == null || item.data == null) continue;

            if (Inventory.ContainsKey(item.data))
                Inventory[item.data] += item.quantity;
            else
                Inventory[item.data] = item.quantity;
        }

        // Step 3: Add items from Shortcut Slots
        if (ShortcutSlotParent != null)
        {
            foreach (Transform child in ShortcutSlotParent)
            {
                ShortCutSlot slot = child.GetComponent<ShortCutSlot>();
                if (slot == null || slot.AssignedItem == null || slot.AssignedItem.data == null)
                    continue;

                ItemData itemData = slot.AssignedItem.data;
                int quantity = slot.AssignedItem.quantity;

                if (Inventory.ContainsKey(itemData))
                    Inventory[itemData] += quantity;
                else
                    Inventory[itemData] = quantity;
            }
        }
    }

    void GenerateSlot(CraftableItem item, bool FullyCraftable)
    {
        GameObject slot = Instantiate(CraftSlot, SlotParent.transform);
        slot.transform.localScale = Vector3.one;
        slot.transform.localPosition = Vector3.zero;

        // Slot UI
        Slot slotComponent = slot.GetComponent<Slot>();
        slotComponent.SetImage(item.GetItemData().itemIconIU);
        slotComponent.QuantityHolder.SetActive(false);

        // Highlight
        slotComponent.SetActiveColor(FullyCraftable);

        // Button click
        Button btn = slot.GetComponent<Button>();
        if (btn != null)
        {
            btn.interactable = FullyCraftable; // Only clickable if fully craftable
            btn.onClick.RemoveAllListeners();
            ItemData capturedItem = item.GetItemData();
            btn.onClick.AddListener(() => OnCraftSlotClicked(capturedItem, slotComponent, FullyCraftable));
        }

        // Tooltip
        TooltipTrigger trigger = slot.GetComponent<TooltipTrigger>();
        if (trigger != null)
        {
            string name = item.GetItemData().itemName;
            string desc = GenerateRequiredAndAcquiredText(item);
            trigger.Initialize(name, desc);
        }

        // Auto re-select only if fully craftable
        if (SelectedItem != null && SelectedItem == item.GetItemData() && FullyCraftable)
        {
            OnCraftSlotClicked(SelectedItem, slotComponent, FullyCraftable);
        }
    }

    string GenerateRequiredAndAcquiredText(CraftableItem craftItem)
    {
        string result = "Item Required:\n";
        foreach (CraftRequirement req in craftItem.requirements)
        {
            Inventory.TryGetValue(req.item, out int owned);
            result += $"{req.item.itemName}: {owned}/{req.quantity}\n";
        }
        result += "\n" + TooltipUI.Instance.GenerateTooltipText(craftItem.GetItemData());
        return result;
    }
    #endregion

    #region Crafting
    void OnCraftSlotClicked(ItemData item, Slot slot, bool FullyCraftable)
    {
        bool ItemSelected = slot.SetSelectedImage(true, SelectedSlot);
        if (ItemSelected)
        {
            SelectedSlot = slot;
            SelectedItem = item;
            CraftButton.SetActive(FullyCraftable); // Only enable if fully craftable
        }
        else
        {
            SelectedSlot = null;
            SelectedItem = null;
            CraftButton.SetActive(false);
        }
    }

    public void OnCraftClicked()
    {
        if (SelectedItem == null) return;

        CraftableItem craftItem = SelectedItem.lootPrefab.GetComponent<CraftableItem>();
        if (craftItem == null) return;

        // Check if player has enough items total (inventory + shortcut)
        bool canCraft = true;
        foreach (CraftRequirement req in craftItem.requirements)
        {
            int totalOwned = GetTotalItemCount(req.item);
            if (totalOwned < req.quantity)
            {
                canCraft = false;
                break;
            }
        }

        if (!canCraft)
        {
            Debug.Log("Not enough materials to craft " + SelectedItem.itemName);
            return;
        }

        // ✅ Add crafted item to inventory
        bool crafted = playerInventory.AddItem(SelectedItem);
        if (crafted)
        {
            // 🔧 Remove required items from both sources (inventory → shortcuts)
            foreach (CraftRequirement req in craftItem.requirements)
            {
                ConsumeItem(req.item, req.quantity);
            }
        }

        // Refresh UI
        LoadCraftableItems();
    }

    int GetTotalItemCount(ItemData item)
    {
        int total = 0;

        // Count from inventory
        foreach (var i in playerInventory.collectedItems)
        {
            if (i.data == item)
                total += i.quantity;
        }

        // Count from shortcut slots
        foreach (Transform child in ShortcutSlotParent)
        {
            ShortCutSlot slot = child.GetComponent<ShortCutSlot>();
            if (slot?.AssignedItem != null && slot.AssignedItem.data == item)
                total += slot.AssignedItem.quantity;
        }

        return total;
    }

    void ConsumeItem(ItemData item, int amountNeeded)
    {
        // Step 1: Consume from inventory first
        for (int i = 0; i < playerInventory.collectedItems.Count && amountNeeded > 0; i++)
        {
            ItemInstance invItem = playerInventory.collectedItems[i];
            if (invItem.data == item)
            {
                int amountToTake = Mathf.Min(invItem.quantity, amountNeeded);
                invItem.quantity -= amountToTake;
                amountNeeded -= amountToTake;

                // Remove empty entries
                if (invItem.quantity <= 0)
                {
                    playerInventory.collectedItems.RemoveAt(i);
                    i--; // Adjust index after removal
                }
            }
        }

        // Step 2: If still missing, consume from shortcuts
        if (amountNeeded > 0)
        {
            foreach (Transform child in ShortcutSlotParent)
            {
                ShortCutSlot slot = child.GetComponent<ShortCutSlot>();
                if (slot?.AssignedItem == null || slot.AssignedItem.data != item)
                    continue;

                int amountToTake = Mathf.Min(slot.AssignedItem.quantity, amountNeeded);
                slot.AssignedItem.quantity -= amountToTake;
                amountNeeded -= amountToTake;

                // Update UI
                slot.SetDetail(slot.AssignedItem);

                // Clear slot if empty
                if (slot.AssignedItem.quantity <= 0)
                    slot.OnReset();

                if (amountNeeded <= 0)
                    break;
            }
        }
    }

    #endregion
}
