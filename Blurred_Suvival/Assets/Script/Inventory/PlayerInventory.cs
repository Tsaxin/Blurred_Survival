using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Inventory Settings")]
    public int maxInventorySize = 10; // You can tweak this in the Inspector

    public List<ItemInstance> collectedItems = new List<ItemInstance>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Avoid duplicate
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Adds an item to the inventory (weapon, consumable, etc.).
    /// Handles stacking for consumables.
    /// </summary>
    /// <param name="newItem">The ItemData to add</param>
    /// <param name="quantity">Quantity to add (default 1)</param>
    /// <returns>True if added successfully, false if inventory full.</returns>
    public bool AddItem(ItemData newItem, int quantity = 1)
    {
        if (newItem == null || quantity <= 0)
            return false;

        SFXManager.Instance?.PlayPickUpSound();

        if (newItem.itemType == ItemType.Ration)
        {
            RationData ration = newItem as RationData;
            HungerManager.Instance.RestoreHunger(ration.RationRestoreAmount);
            TextNotification.Instance.EnqueueCollectedText($"{ration.RationRestoreAmount}x ration collected.", TextNotification.FloatingTextType.Heal);
            return true;
        }

        // ✅ Try adding to shortcut first
        int remaining = AddToShortCut(new ItemInstance(newItem, quantity));

        // ✅ If added fully to shortcut — show floating text and exit
        if (remaining == 0)
        {
            TextNotification.Instance?.EnqueueCollectedText($"{newItem.itemName} added to shortcut.", TextNotification.FloatingTextType.Heal);
            return true;
        }

        bool isStackable = StackableItem.IsStackable(newItem.itemType);

        if (isStackable)
        {
            // Fill existing stacks first
            foreach (var stack in collectedItems)
            {
                if (stack.data == newItem && stack.quantity < newItem.MaxQuantity)
                {
                    int space = newItem.MaxQuantity - stack.quantity;
                    int toAdd = Mathf.Min(space, remaining);
                    stack.quantity += toAdd;
                    remaining -= toAdd;

                    if (remaining <= 0)
                    {
                        TextNotification.Instance?.EnqueueCollectedText($"{stack.data.itemName} collected.", TextNotification.FloatingTextType.Heal);
                        return true;
                    }
                }
            }
        }

        // Add new stacks or individual weapons
        while (remaining > 0)
        {
            if (collectedItems.Count >= maxInventorySize)
            {
                // ✅ Try adding to empty shortcut slots as fallback
                bool AddedToShortcut = AddToNewShortCut(new ItemInstance(newItem, remaining));

                if (AddedToShortcut)
                {
                    TextNotification.Instance?.EnqueueCollectedText($"{newItem.itemName} added to shortcut.", TextNotification.FloatingTextType.Heal);
                    return true;
                }

                TextNotification.Instance?.EnqueueCollectedText("Inventory Full!!", TextNotification.FloatingTextType.Damage);
                return false;
            }

            int toAdd = isStackable ? Mathf.Min(remaining, newItem.MaxQuantity) : 1;
            collectedItems.Add(new ItemInstance(newItem, toAdd));
            remaining -= toAdd;
        }

        // ✅ Final catch-all notification
        TextNotification.Instance?.EnqueueCollectedText($"{newItem.itemName} collected.", TextNotification.FloatingTextType.Heal);
        return true;
    }


    /// <summary>
    /// Removes a certain quantity of an item from the inventory.
    /// If quantity is zero or less, removes the whole stack.
    /// </summary>
    public bool RemoveItem(ItemData item, bool DropToTile = true)
    {
        ItemInstance instance = collectedItems.Find(i => i.data == item);
        if (instance == null)
        {
            return false;
        }

        // Remove ALL quantity
        collectedItems.Remove(instance);

        // ✅ Drop item back on tile
        if (DropToTile && TileManager.Instance != null)
        {
            TileData dropTile = TileManager.Instance.GetBestTileForDrop();
            if (dropTile != null)
            {
                if (item.lootPrefab != null)
                {
                    GameObject loot = GameObject.Instantiate(item.lootPrefab);
                    if (item.itemType != ItemType.Weapon)
                    {
                        TextNotification.Instance.EnqueueCollectedText($"{instance.quantity}x {item.itemName} dropped!", TextNotification.FloatingTextType.Damage);
                    }
                    else
                    {
                        TextNotification.Instance.EnqueueCollectedText($"{item.itemName} dropped!", TextNotification.FloatingTextType.Damage);
                    }
                    dropTile.PlaceLoot(loot);
                }
                else
                {
                    Debug.LogWarning($"Item {item.name} has no loot prefab assigned!");
                }
            }
            else
            {
                Debug.LogWarning("No available tile found for item drop.");
            }
        }

        InventoryUIManager.Instance?.RefreshInventory();
        return true;
    }

    public void RemoveItemWithQuantity(ItemData item, int quantity = 1)
    {
        ItemInstance instance = collectedItems.Find(i => i.data == item);
        if (instance != null)
        {
            instance.quantity -= quantity;
            if (instance.quantity <= 0)
            {
                collectedItems.Remove(instance);
            }
        }
    }

    #region ShortCutSlots
    [Header("ShortCut")]
    public Transform ShortcutParent;

    public void DragAndDropSlot(ShortCutSlot slot, int index, DraggableSlot draggableSlot)
    {
        if (slot != null)
        {
            var itemInstance = collectedItems[index];

            if (slot.AssignedItem != null)
            {
                var slotItem = slot.AssignedItem.data;
                var slotQty = slot.AssignedItem.quantity;

                if (itemInstance.data.itemName == slotItem.itemName && StackableItem.IsStackable(itemInstance.data.itemType))
                {
                    int total = slotQty + itemInstance.quantity;
                    if (total > 20)
                    {
                        int remainder = total - 20;
                        slot.SetOnClick(new ItemInstance(slotItem, 20), TurnManager.Instance?.SelectedUnit);
                        collectedItems[index].quantity = remainder;
                    }
                    else
                    {
                        slot.SetOnClick(new ItemInstance(slotItem, total), TurnManager.Instance?.SelectedUnit);
                        collectedItems.RemoveAt(index);
                    }
                }
                else
                {
                    slot.SetOnClick(itemInstance, TurnManager.Instance?.SelectedUnit);
                    collectedItems.RemoveAt(index);
                    AddItem(slotItem, slotQty);
                }
            }
            else
            {
                slot.SetOnClick(itemInstance, TurnManager.Instance?.SelectedUnit);
                collectedItems.RemoveAt(index);
            }

            InventoryUIManager.Instance.RefreshInventory();
            Destroy(draggableSlot.gameObject);
        }
    }

    public int AddToShortCut(ItemInstance itemInstance)
    {
        int remainder = itemInstance.quantity;
        foreach (Transform child in ShortcutParent)
        {
            ShortCutSlot shortCutSlot = child.GetComponent<ShortCutSlot>();
            if (shortCutSlot.AssignedItem != null && shortCutSlot.AssignedItem.data.itemName == itemInstance.data.itemName && StackableItem.IsStackable(itemInstance.data.itemType))
            {
                int total = shortCutSlot.AssignedItem.quantity + remainder;
                if (total > 20)
                {
                    shortCutSlot.AssignedItem.quantity = 20;
                    remainder = total - 20;
                    shortCutSlot.SetDetail(shortCutSlot.AssignedItem);
                }
                else
                {
                    shortCutSlot.AssignedItem.quantity += remainder;
                    remainder = 0;
                    shortCutSlot.SetDetail(shortCutSlot.AssignedItem);
                    return remainder;
                }
            }
        }

        return remainder;
    }

    public bool AddToNewShortCut(ItemInstance itemInstance)
    {
        foreach (Transform child in ShortcutParent)
        {
            ShortCutSlot shortCutSlot = child.GetComponent<ShortCutSlot>();
            if (shortCutSlot.AssignedItem == null)
            {
                shortCutSlot.SetOnClick(itemInstance, null);

                return true;
            }
        }

        return false;
    }
    #endregion
}
