using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class WeaponData : ItemData
{
    public int attackBoost;
    public int attackCountBoost;
    public int defenseBoost;
    public int movementBoost;
    public int healthBoost;
    public int rangeBoost;
    public int criticalBoost;
    public int evasionBoost;
    public enum Type
    {
        Melee,
        Range,
        Throwable
    }

    public Type type;

    public StatModifier GetModifier()
    {
        return new StatModifier
        {
            attack = attackBoost,
            range = rangeBoost,
            defense = defenseBoost,
            attackCount = attackCountBoost,
            health = healthBoost,
            movement = movementBoost,
            critical = criticalBoost,
            evasion = evasionBoost
        };
    }
}


