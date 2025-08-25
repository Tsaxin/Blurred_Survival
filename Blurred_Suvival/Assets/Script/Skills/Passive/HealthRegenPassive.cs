using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Passives/HealthRegen")]
public class HealthRegenPassive : PassiveSkill
{
    public int regenAmount;

    public bool Individual = true;
    public override PassiveTrigger Trigger => PassiveTrigger.OnTurnStart;

    public override void OnTurnStart(CharacterStats owner, Transform Characters)
    {
        if (Individual)
        {
            owner.Heal(regenAmount);
        }
        else
        {
            foreach (Transform character in Characters) {
                character.GetComponent<CharacterStats>().Heal(regenAmount);
            }
        }
    }
}
