using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearUI : MonoBehaviour
{
    public static GearUI Instance;
    public Transform HelmetParent, ArmorParent, PantParent, ShoeParent, WeaponParent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public GameObject Panel;
    public GameObject Slot;
    public void OnShowGear(GearEquipper gearEquipper)
    {
        DestroyChildren(HelmetParent);
        SetWeaponDetail();
        Panel.SetActive(true);
    }

    public void OnHideGear()
    {
        Panel.SetActive(false);
    }

    void SetWeaponDetail()
    {
        var gearEquipper = TurnManager.Instance.SelectedUnit.GetComponent<GearEquipper>();

        if (gearEquipper.equippedWeapon != null)
        {
            // Create UI slot
            GameObject obj = Instantiate(Slot, WeaponParent.position, Quaternion.identity, WeaponParent);

            // Prepare tooltip data
            ItemInstance tempInstance = new ItemInstance(gearEquipper.equippedWeapon, 1);
            string tooltipText = InventoryUIManager.Instance.GenerateTooltipText(tempInstance);

            // Assign tooltip trigger
            TooltipTrigger trigger = obj.GetComponent<TooltipTrigger>();
            if (trigger != null)
            {
                trigger.Initialize(gearEquipper.equippedWeapon.itemName, tooltipText);
            }
        }
    }

    void DestroyAllChildren()
    {
        DestroyChildren(HelmetParent);
        DestroyChildren(ArmorParent);
        DestroyChildren(ShoeParent);
        DestroyChildren(PantParent);
        DestroyChildren(WeaponParent);
    }

    void DestroyChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}
