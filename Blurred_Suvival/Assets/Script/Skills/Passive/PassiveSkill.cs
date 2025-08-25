using UnityEngine;

public enum PassiveTrigger
{
    Always,
    OnBattleStart,
    OnTurnStart
}

public abstract class PassiveSkill : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    public bool IsUpgradable = false;

    public abstract PassiveTrigger Trigger { get; }

    // Called once at game load to modify base stats
    public virtual void ApplyPassiveStats(CharacterStats stats) { }

    // Called at start of battle
    public virtual void OnBattleStart(CharacterStats owner, CharacterStats[] allies, CharacterStats[] enemies) { }

    // Called at start of owner's turn
    public virtual void OnTurnStart(CharacterStats owner,Transform Character) { }
}

[System.Serializable]
public class PassiveSkillObject
{
    public PassiveSkill passiveSkill;
    public int Level;
    public int LevelUpFactor;
}
