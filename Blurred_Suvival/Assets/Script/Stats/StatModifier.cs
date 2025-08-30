[System.Serializable]
public struct StatModifier
{
    public int attack;
    public int range;
    public float defense;
    public int attackCount;
    public int health;
    public int movement;
    public float critical;
    public float evasion;

    public static StatModifier operator +(StatModifier a, StatModifier b)
    {
        return new StatModifier
        {
            attack = a.attack + b.attack,
            range = a.range + b.range,
            defense = a.defense + b.defense,
            attackCount = a.attackCount + b.attackCount,
            health = a.health + b.health,
            movement = a.movement + b.movement,
            critical = a.critical + b.critical,
            evasion = a.evasion + b.evasion,
        };
    }
}
