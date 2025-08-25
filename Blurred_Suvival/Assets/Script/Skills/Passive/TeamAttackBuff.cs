using UnityEngine;

[CreateAssetMenu(menuName = "Passives/TeamAttackBuff")]
public class TeamAttackBuff : PassiveSkill
{
    public int attackIncrease;

    public override PassiveTrigger Trigger => PassiveTrigger.OnBattleStart;

    public override void OnBattleStart(CharacterStats owner, CharacterStats[] allies, CharacterStats[] enemies)
    {
        foreach (var ally in allies)
        {
            ally.attack += attackIncrease;
        }
    }
}
