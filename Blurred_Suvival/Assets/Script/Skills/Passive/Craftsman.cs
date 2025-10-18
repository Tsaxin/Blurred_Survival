using UnityEngine;

[CreateAssetMenu(fileName = "Craftsman", menuName = "PassiveSkills/Craftsman")]
public class Craftsman : PassiveSkill
{
    public override PassiveTrigger Trigger => PassiveTrigger.Always;
}
