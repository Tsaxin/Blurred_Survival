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
    public void OnItemSlotClicked(ItemInstance clickedItem, CharacterController selectedUnit, bool fromShortcut = false,ShortCutSlot Slot=null)
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
                    (gear, data) => gear.EquipWeapon(data), fromShortcut,Slot);
                break;

            case ItemType.Helmet:
                HandleEquipmentClick(clickedItem, selectedUnit,
                    gear => gear.equippedHelmet,
                    (gear, data) => gear.EquipHelmet(data), fromShortcut,Slot);
                break;

            case ItemType.Vest:
                HandleEquipmentClick(clickedItem, selectedUnit,
                    gear => gear.equippedVest,
                    (gear, data) => gear.EquipVest(data), fromShortcut,Slot);
                break;

            case ItemType.Trouser:
                HandleEquipmentClick(clickedItem, selectedUnit,
                    gear => gear.equippedTrouser,
                    (gear, data) => gear.EquipTrouser(data), fromShortcut,Slot);
                break;

            case ItemType.Shoe:
                HandleEquipmentClick(clickedItem, selectedUnit,
                    gear => gear.equippedShoe,
                    (gear, data) => gear.EquipShoe(data), fromShortcut,Slot);
                break;

            case ItemType.Consumable:
                HandleConsumableClick(clickedItem, selectedUnit, fromShortcut,Slot);
                break;
        }

        if (!fromShortcut) InventoryUIManager.Instance.RefreshInventory();
        //selectedUnit.ShowAvailableMoveTiles();
        selectedUnit.FinishedTurn();

        if (!fromShortcut) InventoryUIManager.Instance.CloseInventory();
    }


    /// <summary>
    /// Generic handler for equippable items (Weapon, Helmet, etc.)
    /// </summary>
    private void HandleEquipmentClick(ItemInstance clickedItem, CharacterController selectedUnit,
                                      System.Func<GearEquipper, WeaponData> getEquippedItem,
                                      System.Action<GearEquipper, WeaponData> equipAction, bool IsFromShortCut,ShortCutSlot Slot=null)
    {
        var gearEquipper = selectedUnit.GetComponent<GearEquipper>();
        if (gearEquipper == null)
        {
            Debug.LogWarning("Selected character has no GearEquipper.");
            return;
        }

        SFXManager.Instance?.PlayEquipSound();

        // Remove the clicked item from inventory
        if (!IsFromShortCut)
        {
            playerInventory.collectedItems.Remove(clickedItem);

            // Return currently equipped item to inventory (if any)
            var currentlyEquipped = getEquippedItem(gearEquipper);
            if (currentlyEquipped != null)
            {
                playerInventory.collectedItems.Add(new ItemInstance(currentlyEquipped));
            }
        }
        else
        {
            Slot.OnReset();

            // Return currently equipped item to inventory (if any)
            var currentlyEquipped = getEquippedItem(gearEquipper);
            if (currentlyEquipped != null)
            {
                Slot.SetOnClick(new ItemInstance(currentlyEquipped),selectedUnit);
            }
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
    private void HandleConsumableClick(ItemInstance clickedItem, CharacterController selectedUnit, bool fromShortcut,ShortCutSlot Slot=null)
    {
        SFXManager.Instance.PlaySFX(clickedItem.data.useSound);
        var consumableData = (ConsumableData)clickedItem.data;
        var characterStats = selectedUnit.GetComponent<CharacterStats>();
        if (characterStats == null)
        {
            Debug.LogWarning("Selected character has no CharacterStats.");
            return;
        }

        // Heal or apply effect
        characterStats.Heal(consumableData.healthRestoreAmount);

        // Reduce quantity or remove item
        clickedItem.quantity--;
        if (fromShortcut)
        {
            if (clickedItem.quantity <= 0)
            {
                Slot.OnReset();
            }
            else
            {   
                Slot.SetDetail(clickedItem);  
            }
        }
        if (clickedItem.quantity <= 0)
        {
            playerInventory.collectedItems.Remove(clickedItem);
        }
    }

}
