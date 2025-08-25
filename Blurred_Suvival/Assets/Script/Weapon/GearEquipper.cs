using Unity.VisualScripting;
using UnityEngine;

public class GearEquipper : MonoBehaviour
{
    public WeaponData equippedWeapon;

    private CharacterStats stats;

    void Start()
    {
        stats = GetComponent<CharacterStats>();
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        if (stats == null)
        {
            Debug.LogWarning("CharacterStats component missing!");
            return;
        }

        // Remove old weapon boosts if equipped
        if (equippedWeapon != null)
        {
            RemoveWeaponStats(equippedWeapon);
        }

        // Equip new weapon
        equippedWeapon = newWeapon;

        if (newWeapon != null)
        {
            ApplyWeaponStats(newWeapon);
        }
        Debug.Log("Empty Handed");

        stats.RecalculateStats();
    }

    public void LoadWeaponSprite()
    {
        if (equippedWeapon != null)
        {
            if (equippedWeapon.type == WeaponData.Type.Melee)
            {
                GetComponent<CharacterController>().SetMeleeWeapon(equippedWeapon.itemIcon);
            }
            else if (equippedWeapon.type == WeaponData.Type.Range)
            {
                GetComponent<CharacterController>().SetRangeWeapon(equippedWeapon.itemIcon);
            }
        }
        else
        {
            GetComponent<CharacterController>().SetFist();
        }
    }

    private void ApplyWeaponStats(WeaponData weapon)
    {
        stats.attack += weapon.attackBoost;
        stats.range += weapon.rangeBoost;
        stats.Defense += weapon.defenseBoost;
        stats.AttackCount += weapon.attackCountBoost;
        stats.maxHealth += weapon.healthBoost;
        stats.MovementRange += weapon.movementBoost;
        stats.CriticalChance += weapon.criticalBoost;
        stats.EvasionChance += weapon.evasionBoost;
    }

    private void RemoveWeaponStats(WeaponData weapon)
    {
        stats.attack -= weapon.attackBoost;
        stats.range -= weapon.rangeBoost;
        stats.Defense -= weapon.defenseBoost;
        stats.AttackCount -= weapon.attackCountBoost;
        stats.maxHealth -= weapon.healthBoost;
        stats.MovementRange -= weapon.movementBoost;
        stats.CriticalChance -= weapon.criticalBoost;
        stats.EvasionChance -= weapon.evasionBoost;
    }
}
