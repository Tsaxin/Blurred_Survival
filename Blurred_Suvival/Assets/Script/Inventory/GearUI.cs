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
        string tooltipText = InventoryUIManager.Instance.GenerateTooltipText(tempInstance);

        // Assign tooltip trigger
        TooltipTrigger trigger = obj.GetComponent<TooltipTrigger>();
        if (trigger != null)
        {
            trigger.Initialize(gearData.itemName, tooltipText);
        }

        obj.GetComponent<Image>().sprite = gearData.itemIconIU;
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
