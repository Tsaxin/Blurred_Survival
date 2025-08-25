using UnityEngine;

[CreateAssetMenu(menuName = "Passives/FlatStatBoost")]
public class FlatStatBoost : PassiveSkill
{
    public int attackBoost;
    public int defenseBoost;
    public int healthBoost;

    public override PassiveTrigger Trigger => PassiveTrigger.Always;

    public override void ApplyPassiveStats(CharacterStats stats)
    {
        stats.attack += attackBoost;
        stats.Defense += defenseBoost;
        stats.maxHealth += healthBoost;
    }
}
