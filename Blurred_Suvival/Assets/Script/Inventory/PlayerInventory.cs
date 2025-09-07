using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Inventory Settings")]
    public int maxInventorySize = 10; // You can tweak this in the Inspector

    public List<ItemInstance> collectedItems = new List<ItemInstance>();

    public Transform TextGenerationPoint;

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

        if (newItem.itemType == ItemType.Ration)
        {
            RationData ration = newItem as RationData;
            HungerManager.Instance.RestoreHunger(ration.RationRestoreAmount);
            EnqueueCollectedText($"{ration.RationRestoreAmount}x ration collected.", FloatingTextType.Heal);

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
                        EnqueueCollectedText($"{stack.data.itemName} collected.", FloatingTextType.Heal);
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
                EnqueueCollectedText("Inventory Full!!", FloatingTextType.Damage);
                return false; // inventory full, some items not added
            }

            int toAdd = isStackable ? Mathf.Min(remaining, newItem.MaxQuantity) : 1;
            collectedItems.Add(new ItemInstance(newItem, toAdd));
            remaining -= toAdd;
        }
        EnqueueCollectedText($"{newItem.itemName} collected.", FloatingTextType.Heal);
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
                    EnqueueCollectedText($"{instance.quantity}x {item.itemName} dropped!", FloatingTextType.Damage);
                }
                else
                {
                    EnqueueCollectedText($"{item.itemName} dropped!", FloatingTextType.Damage);
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


    #region Notification Message
    public enum FloatingTextType
    {
        Damage,
        Heal, // for things like "Inventory Full"
    }
    [Header("Notification Message")]
    private Queue<(string message, FloatingTextType type)> floatingTextQueue
    = new Queue<(string, FloatingTextType)>();

    private string NoSpaceInInventory = "";
    private bool isShowingText = false;

    public float MessageInterval = 1f;

    public void EnqueueCollectedText(string msg, FloatingTextType type)
    {
        floatingTextQueue.Enqueue((msg, type));

        if (!isShowingText)
            StartCoroutine(ProcessFloatingTextQueue());
    }

    private IEnumerator ProcessFloatingTextQueue()
    {
        isShowingText = true;
        Vector3 basePosition = TextGenerationPoint.position;

        while (floatingTextQueue.Count > 0)
        {
            var entry = floatingTextQueue.Dequeue();
            Vector3 spawnPos = basePosition + Vector3.up;

            switch (entry.type)
            {
                case FloatingTextType.Damage:
                    DamageTextManager.Instance.ShowDamage(spawnPos, entry.message, false);
                    break;

                case FloatingTextType.Heal:
                    DamageTextManager.Instance.ShowHeal(spawnPos, entry.message);
                    break;
            }

            yield return new WaitForSeconds(MessageInterval);
        }

        isShowingText = false;
    }

    #endregion
}
