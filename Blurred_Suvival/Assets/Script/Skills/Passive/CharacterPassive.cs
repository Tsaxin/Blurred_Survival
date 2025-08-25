using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPassive : MonoBehaviour
{
    public List<PassiveSkillObject> passiveSkills = new List<PassiveSkillObject>();

    public void ApplyPassiveStatBoosts()
    {
        foreach (var skill in passiveSkills)
        {
            if (skill.passiveSkill.Trigger == PassiveTrigger.Always)
                skill.passiveSkill.ApplyPassiveStats(GetComponent<CharacterStats>());
        }
    }

    public void TriggerBattleStartEffects(CharacterStats[] allies, CharacterStats[] enemies)
    {
        foreach (var skill in passiveSkills)
        {
            if (skill.passiveSkill.Trigger == PassiveTrigger.OnBattleStart)
                skill.passiveSkill.OnBattleStart(GetComponent<CharacterStats>(), allies, enemies);
        }
    } 

    public void TriggerTurnStartEffects()
    {
        foreach (var skill in passiveSkills)
        {
            if (skill.passiveSkill.Trigger == PassiveTrigger.OnTurnStart)
                skill.passiveSkill.OnTurnStart(GetComponent<CharacterStats>(),TurnManager.Instance.playerParent.transform);
        }
    }

}
