using UnityEngine;

[CreateAssetMenu(fileName = "BornLeader", menuName = "PassiveSkills/BornLeader")]
public class BornLeaderPassive : PassiveSkill
{
    public override PassiveTrigger Trigger => PassiveTrigger.OnBattleStart;

    public override void OnBattleStart(CharacterStats owner, CharacterStats[] allies, CharacterStats[] enemies)
    {
        //SquadFormationManager.Instance.EnableFormationPersistence();
    }
}
