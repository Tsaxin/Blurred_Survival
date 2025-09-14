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

        MusicManager.Instance?.PlayPickUpSound();

        if (newItem.itemType == ItemType.Ration)
        {
            RationData ration = newItem as RationData;
            HungerManager.Instance.RestoreHunger(ration.RationRestoreAmount);
            TextNotification.Instance.EnqueueCollectedText($"{ration.RationRestoreAmount}x ration collected.", TextNotification.FloatingTextType.Heal);

            return true;
        }

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
                        TextNotification.Instance.EnqueueCollectedText($"{stack.data.itemName} collected.", TextNotification.FloatingTextType.Heal);
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
                TextNotification.Instance.EnqueueCollectedText("Inventory Full!!", TextNotification.FloatingTextType.Damage);
                return false; // inventory full, some items not added
            }

            int toAdd = isStackable ? Mathf.Min(remaining, newItem.MaxQuantity) : 1;
            collectedItems.Add(new ItemInstance(newItem, toAdd));
            remaining -= toAdd;
        }
        TextNotification.Instance.EnqueueCollectedText($"{newItem.itemName} collected.", TextNotification.FloatingTextType.Heal);
        return true;
    }

    /// <summary>
    /// Removes a certain quantity of an item from the inventory.
    /// If quantity is zero or less, removes the whole stack.
    /// </summary>
    public bool RemoveItem(ItemData item)
    {
        ItemInstance instance = collectedItems.Find(i => i.data == item);
        if (instance == null)
        {
            return false;   
        }

        // Remove ALL quantity
        collectedItems.Remove(instance);

        // ✅ Drop item back on tile
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

        InventoryUIManager.Instance.RefreshInventory();
        return true;
    }
}
