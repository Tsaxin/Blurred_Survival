using System.Collections.Generic;
using TMPro;
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

    public float AttackSpeed = 1;

    [Header("Splash Setting")]
    public float SplashAccuracy;
    public int SplashDamage;

    public enum Type
    {
        Melee,
        Range,
        Throwable
    }

    public enum AttackPattern
    {
        SingleTarget,
        SlingshotSplash,
        // Later: Cone, Line, Pierce, etc.
    }

    public AttackPattern attackPattern;

    public Type type;

    public List<AudioClip> audioClip;

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


