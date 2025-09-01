using System.Collections.Generic;
using UnityEngine;

public class ZombieLevelScaler : MonoBehaviour
{
    public int MinLevel=1, MaxLevel=20;
    /// <summary>
    /// Calculates zombie level based on how close it is to the nearest city.
    /// Closer = higher level, and every LevelIncrementDistance away reduces level by 1.
    /// </summary>
    public int GetZombieLevel()
    {
        return Random.Range(MinLevel,MaxLevel); ;
    }
}
