using System.Collections.Generic;
using UnityEngine;

public class ZombieLevelScaler : MonoBehaviour
{
    public static ZombieLevelScaler Instance { get; private set; }

    [Header("City References")]
    public List<GameObject> cities;

    [Header("Level Scaling Settings")]
    public float LevelIncrementDistance = 0.5f; // Every X units farther reduces level by 1
    public int maxLevel = 50;
    public int minLevel = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Calculates zombie level based on how close it is to the nearest city.
    /// Closer = higher level, and every LevelIncrementDistance away reduces level by 1.
    /// </summary>
    public int ScaleZombieLevel(GameObject squad)
    {
        if (squad == null || cities == null || cities.Count == 0)
        {
            Debug.LogWarning("Zombie or cities list is null/empty.");
            return minLevel;
        }

        float shortestDistance = float.MaxValue;
        Vector3 squadPos = squad.transform.position;

        foreach (var city in cities)
        {
            if (city == null) continue;
            float dist = Vector3.Distance(squadPos, city.transform.position);
            if (dist < shortestDistance)
                shortestDistance = dist;
        }

        int decrements = Mathf.FloorToInt(shortestDistance / LevelIncrementDistance);
        int level = Mathf.Clamp(maxLevel - decrements, minLevel, maxLevel);

        Debug.Log($"Zombie near city: distance = {shortestDistance:F2}, level = {level}");

        return level;
    }
}
