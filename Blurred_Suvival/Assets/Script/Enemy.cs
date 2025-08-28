using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public TileManager tileManager;

    // List of currently active/enabled enemies
    public List<GameObject> spawnedEnemies = new List<GameObject>();

    public List<GameObject> GetSpawnedEnemies() => spawnedEnemies;

    public List<GameObject> Zombies;

    public int MaxIndividualCount;
    public Transform ZombiePool;

    public static Enemy Instance;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    [ContextMenu("Generate Zombies")]
    public void SpawnZombie()
    {
        for (int i = ZombiePool.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(ZombiePool.transform.GetChild(i).gameObject);
        }

        foreach (GameObject child in Zombies)
        {
            for (int j = 0; j < MaxIndividualCount; j++)
            {
                GameObject Object = Instantiate(child, ZombiePool.transform.position, Quaternion.identity);
                Object.transform.SetParent(ZombiePool.transform);

                Object.GetComponent<ZombieAIBase>().tileManager = tileManager;
                Object.GetComponent<ZombieAIBase>().EnemyManager = this;

            }
        }
    }

    public void SpawnZombie(string ZombieName)
    {
        foreach (GameObject child in Zombies)
        {
            if (child.GetComponent<CharacterStats>().CharacterName == ZombieName)
            {
                GameObject Object = Instantiate(child, ZombiePool.transform.position, Quaternion.identity);
                Object.transform.SetParent(ZombiePool.transform);

                Object.GetComponent<ZombieAIBase>().tileManager = tileManager;
                Object.GetComponent<ZombieAIBase>().EnemyManager = this;
                break;
            }
        }
    }

    public void DestroyAllChildren()
    {
        // Copy children to a temp list to avoid modifying during iteration
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            ZombieAIBase ai = child.GetComponent<MonoBehaviour>() as ZombieAIBase;
            if (ai != null && ai.EnemyManager != null)
            {
                ai.EnemyManager.SpawnZombie(ai.GetComponent<CharacterStats>().CharacterName);
            }
            else
            {
                Debug.LogWarning("ZombieAIBase or its EnemyManager not found on child.");
            }
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            Destroy(child.gameObject);
        }

        CheckSpawnListAnamoly();
    }

    void CheckSpawnListAnamoly()
    {
        spawnedEnemies.RemoveAll(child => child == null);
    }
}
