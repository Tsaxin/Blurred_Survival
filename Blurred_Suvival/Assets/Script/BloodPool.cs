using UnityEngine;
using System.Collections.Generic;

public class BloodPool : MonoBehaviour
{
    public static BloodPool Instance { get; private set; }
    public List<GameObject> pooledObjects;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Gets the next available pooled object (or null if all are in use).
    /// </summary>
    private GameObject GetPooledObject()
    {
        foreach (GameObject obj in pooledObjects)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        return null; // Pool exhausted
    }

    /// <summary>
    /// Spawn blood effect at a target transform's position.
    /// </summary>
    public float MaxYOffset = 1f, MinYOffset = 0f;
    public GameObject SpawnBlood(Transform target)
    {
        GameObject blood = GetPooledObject();
        if (blood != null)
        {
            // Find sprite renderer in target's children
            SpriteRenderer targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
            if (targetRenderer != null)
            {
                // Get blood's ParticleSystem renderer
                ParticleSystemRenderer psr = blood.GetComponent<ParticleSystemRenderer>();
                if (psr != null)
                {
                    psr.sortingOrder = targetRenderer.sortingOrder + 1;
                }
            }

            // Apply position with Y offset
            Vector3 newPos = target.position;
            // Bias factor: higher exponent = stronger bias toward MaxYOffset
            float t = Random.value;              // uniform 0–1
            t = Mathf.Pow(t, 0.3f);              // skew toward 1
            float offset = Mathf.Lerp(MinYOffset, MaxYOffset, t);

            newPos.y += offset;

            blood.transform.position = newPos;

            blood.SetActive(true);
        }
        return blood;
    }
}
