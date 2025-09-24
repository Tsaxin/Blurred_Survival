using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GearUI : MonoBehaviour
{
    public static GearUI Instance;
    public Transform HelmetParent, ArmorParent, PantParent, ShoeParent, WeaponParent;

    public Image CharacterImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public GameObject Panel;
    public GameObject Slot;

    public void OnShowGear(GearEquipper gearEquipper, Sprite sprite)
    {
        CharacterImage.sprite = sprite;
        CharacterImage.SetNativeSize();
        DestroyAllChildren();
        SetGearDetails(gearEquipper);
        Panel.SetActive(true);
    }

    public void OnHideGear()
    {
        Panel.SetActive(false);
    }

    private void SetGearDetails(GearEquipper gearEquipper)
    {
        AddGearSlot(gearEquipper.equippedWeapon, WeaponParent);
        AddGearSlot(gearEquipper.equippedHelmet, HelmetParent);
        AddGearSlot(gearEquipper.equippedVest, ArmorParent);
        AddGearSlot(gearEquipper.equippedTrouser, PantParent);
        AddGearSlot(gearEquipper.equippedShoe, ShoeParent);
    }

    /// <summary>
    /// Creates a UI slot for the given gear if it's equipped.
    /// </summary>
    private void AddGearSlot(WeaponData gearData, Transform parent)
    {
        if (gearData == null) return;

        // Create UI slot
        GameObject obj = Instantiate(Slot, parent.position, Quaternion.identity, parent);

        // Prepare tooltip data
        ItemInstance tempInstance = new ItemInstance(gearData, 1);
        string tooltipText = TooltipUI.Instance.GenerateTooltipText(tempInstance);

        // Assign tooltip trigger
        TooltipTrigger trigger = obj.GetComponent<TooltipTrigger>();
        if (trigger != null)
        {
            trigger.Initialize(gearData.itemName, tooltipText + "\n\nClick to unequip this gear.");
        }

        obj.GetComponent<Image>().sprite = gearData.itemIconIU;

        // Hook up button click → unequip
        Button btn = obj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => UnequipGear(gearData,obj));
        }
    }

    public void UnequipGear(WeaponData gearData,GameObject slot)
    {
        GearEquipper gearEquipper = TurnManager.Instance.SelectedUnit.GetComponent<GearEquipper>();
        // Example: Remove equipped weapon and refresh UI
        if (gearEquipper != null)
        {
            gearEquipper.UnequipItem(gearData);
            PlayerInventory.Instance.AddItem(gearData, 1);
        }

        Destroy(slot);
    }

    private void DestroyAllChildren()
    {
        DestroyChildren(HelmetParent);
        DestroyChildren(ArmorParent);
        DestroyChildren(ShoeParent);
        DestroyChildren(PantParent);
        DestroyChildren(WeaponParent);
    }

    private void DestroyChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}
