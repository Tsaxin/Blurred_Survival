using System.Collections.Generic;
using UnityEngine;

public class GearEquipper : MonoBehaviour
{
    public WeaponData equippedWeapon, equippedHelmet, equippedVest, equippedTrouser, equippedShoe;

    private CharacterStats stats;

    void Start()
    {
        LoadWeaponSprite();
        stats = GetComponent<CharacterStats>();
    }

    public enum EquipmentSlot
    {
        Weapon,
        Helmet,
        Vest,
        Trouser,
        Shoe
    }

    /// <summary>
    /// Generalized method to equip any item into a specific slot.
    /// </summary>
    public void EquipItem(WeaponData newItem, EquipmentSlot slot)
    {
        if (stats == null)
        {
            Debug.LogWarning("CharacterStats component missing!");
            return;
        }

        switch (slot)
        {
            case EquipmentSlot.Weapon:
                equippedWeapon = newItem;
                LoadWeaponSprite();
                break;
            case EquipmentSlot.Helmet:
                equippedHelmet = newItem;
                break;
            case EquipmentSlot.Vest:
                equippedVest = newItem;
                break;
            case EquipmentSlot.Trouser:
                equippedTrouser = newItem;
                break;
            case EquipmentSlot.Shoe:
                equippedShoe = newItem;
                break;
        }

        // Recalculate stats from scratch
        stats.RecalculateStats();
    }

    // Convenience methods
    public void EquipWeapon(WeaponData newWeapon) => EquipItem(newWeapon, EquipmentSlot.Weapon);
    public void EquipHelmet(WeaponData newHelmet) => EquipItem(newHelmet, EquipmentSlot.Helmet);
    public void EquipVest(WeaponData newVest) => EquipItem(newVest, EquipmentSlot.Vest);
    public void EquipTrouser(WeaponData newTrouser) => EquipItem(newTrouser, EquipmentSlot.Trouser);
    public void EquipShoe(WeaponData newShoe) => EquipItem(newShoe, EquipmentSlot.Shoe);

    /// <summary>
    /// Updates the character’s weapon sprite if applicable.
    /// </summary>
    public void LoadWeaponSprite()
    {
        var characterController = GetComponent<CharacterController>();

        if (equippedWeapon != null)
        {
            if (equippedWeapon.type == WeaponData.Type.Melee)
                SetMeleeWeapon(equippedWeapon.itemIcon);
            else if (equippedWeapon.type == WeaponData.Type.Range)
                SetRangeWeapon(equippedWeapon.itemIcon);
        }
        else
        {
            SetFist();
        }
    }

    /// <summary>
    /// Collects all active stat modifiers from equipped gear.
    /// </summary>
    public IEnumerable<StatModifier> GetAllModifiers()
    {
        if (equippedWeapon != null) yield return equippedWeapon.GetModifier();
        if (equippedHelmet != null) yield return equippedHelmet.GetModifier();
        if (equippedVest != null) yield return equippedVest.GetModifier();
        if (equippedTrouser != null) yield return equippedTrouser.GetModifier();
        if (equippedShoe != null) yield return equippedShoe.GetModifier();
    }

        #region WeaponSprite
    public SpriteRenderer Melee, Range, RangeFlash;
    public TrailRenderer TR;
    public void SetWeaponSL(int sortingOrder)
    {
        Melee.sortingOrder = sortingOrder + 2;
        TR.sortingOrder = sortingOrder + 1;
        Range.sortingOrder = sortingOrder + 2;
        RangeFlash.sortingOrder = sortingOrder + 2;
    }

    public void SetRangeWeapon(Sprite sprite)
    {
        Melee.gameObject.SetActive(false);
        Range.gameObject.SetActive(true);
        Range.sprite = sprite;
    }
    public void SetMeleeWeapon(Sprite sprite)
    {
        Melee.enabled = true;
        Melee.gameObject.SetActive(true);
        Range.gameObject.SetActive(false);
        Melee.sprite = sprite;
    }
    public void SetFist()
    {
        Melee.enabled = false;
        Melee.gameObject.SetActive(true);
        Range.gameObject.SetActive(false);
    }
    #endregion
}
