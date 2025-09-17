using UnityEngine;

public class InventoryItemClickHandler : MonoBehaviour
{
    public static InventoryItemClickHandler Instance { get; private set; }

    [SerializeField] private PlayerInventory playerInventory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (playerInventory == null)
            playerInventory = PlayerInventory.Instance;
    }

    /// <summary>
    /// Handles what happens when an item in the inventory UI is clicked.
    /// </summary>
    public void OnItemSlotClicked(ItemInstance clickedItem, CharacterController selectedUnit)
    {
        if (selectedUnit == null)
        {
            Debug.LogWarning("No character selected!");
            return;
        }

        switch (clickedItem.data.itemType)
        {
            case ItemType.Weapon:
                HandleEquipmentClick(clickedItem, selectedUnit, 
                    gear => gear.equippedWeapon, 
                    (gear, data) => gear.EquipWeapon(data));
                break;

            case ItemType.Helmet:
                HandleEquipmentClick(clickedItem, selectedUnit,
                    gear => gear.equippedHelmet,
                    (gear, data) => gear.EquipHelmet(data));
                break;
            
            case ItemType.Vest:
                HandleEquipmentClick(clickedItem, selectedUnit,
                    gear => gear.equippedVest,
                    (gear, data) => gear.EquipVest(data));
                break;

            case ItemType.Trouser:
                HandleEquipmentClick(clickedItem, selectedUnit,
                    gear => gear.equippedTrouser,
                    (gear, data) => gear.EquipTrouser(data));
                break;

            case ItemType.Shoe:
                HandleEquipmentClick(clickedItem, selectedUnit,
                    gear => gear.equippedShoe,
                    (gear, data) => gear.EquipShoe(data));
                break;

            case ItemType.Consumable:
                HandleConsumableClick(clickedItem, selectedUnit);
                break;

            // More item types can be added here
        }

        // Refresh visuals + end turn
        InventoryUIManager.Instance.RefreshInventory();
        selectedUnit.ShowAvailableMoveTiles();
        selectedUnit.FinishedTurn();
        InventoryUIManager.Instance.CloseInventory();
    }

    /// <summary>
    /// Generic handler for equippable items (Weapon, Helmet, etc.)
    /// </summary>
    private void HandleEquipmentClick(ItemInstance clickedItem, CharacterController selectedUnit,
                                      System.Func<GearEquipper, WeaponData> getEquippedItem,
                                      System.Action<GearEquipper, WeaponData> equipAction)
    {
        var gearEquipper = selectedUnit.GetComponent<GearEquipper>();
        if (gearEquipper == null)
        {
            Debug.LogWarning("Selected character has no GearEquipper.");
            return;
        }

        MusicManager.Instance?.PlayEquipSound();

        // Remove the clicked item from inventory
        playerInventory.collectedItems.Remove(clickedItem);

        // Return currently equipped item to inventory (if any)
        var currentlyEquipped = getEquippedItem(gearEquipper);
        if (currentlyEquipped != null)
        {
            playerInventory.collectedItems.Add(new ItemInstance(currentlyEquipped));
        }

        // Equip the new item
        equipAction(gearEquipper, (WeaponData)clickedItem.data);

        TextNotification.Instance.EnqueueCollectedText(
            $"{clickedItem.data.itemName} Equipped!", 
            TextNotification.FloatingTextType.Heal);
    }

    /// <summary>
    /// Handles consumable use.
    /// </summary>
    private void HandleConsumableClick(ItemInstance clickedItem, CharacterController selectedUnit)
    {
        SFXManager.Instance.PlaySFX(clickedItem.data.useSound);
        var consumableData = (ConsumableData)clickedItem.data;
        var characterStats = selectedUnit.GetComponent<CharacterStats>();
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
    }
}
