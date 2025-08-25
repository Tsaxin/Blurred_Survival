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

        bool isStackable = newItem.itemType != ItemType.Weapon; // Only weapons don't stack
        int remaining = quantity;

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
                        Debug.Log($"Added {quantity}x {newItem.itemName} to existing stacks.");
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
                Debug.LogWarning($"Inventory full! Could not add all {newItem.itemName}. Leftover: {remaining}");
                return false; // inventory full, some items not added
            }

            int toAdd = isStackable ? Mathf.Min(remaining, newItem.MaxQuantity) : 1;
            collectedItems.Add(new ItemInstance(newItem, toAdd));
            remaining -= toAdd;
        }
        Debug.Log($"Collected: {newItem.itemName} x{quantity}");
        return true;
    }


    /// <summary>
    /// Removes a certain quantity of an item from the inventory.
    /// If quantity is zero or less, removes the whole stack.
    /// </summary>
    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        ItemInstance instance = collectedItems.Find(i => i.data == item);
        if (instance == null)
            return false;

        if (instance.quantity > quantity)
        {
            instance.quantity -= quantity;
        }
        else
        {
            collectedItems.Remove(instance);
        }

        InventoryUIManager.Instance.RefreshInventory();
        return true;
    }
}
