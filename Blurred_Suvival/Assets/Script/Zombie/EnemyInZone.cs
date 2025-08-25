using UnityEngine;

[System.Serializable]
public class EnemyInZone
{
    public string ZombieName;
    [Range(0f, 1f)]
    public float rarity = 1f; // Likelihood of spawning (0.0 - 1.0)
    public int maxCount = 1;  // Max allowed in this zone
    public int minCount = 0;
}
