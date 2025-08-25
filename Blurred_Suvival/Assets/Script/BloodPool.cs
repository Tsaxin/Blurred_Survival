using UnityEngine;
using System.Collections.Generic;

public class BloodPool : MonoBehaviour
{
    public static BloodPool Instance { get; private set; }

    [Header("Pools")]
    public List<GameObject> BloodPoolObjects;        // Blood particle effects
    public List<GameObject> HugePoolObjects;         // Big particle effects
    public List<GameObject> ExplosionPoolObjects;    // Explosion effects
    public List<GameObject> BloodSplashObjects;      // <-- New: splatter decals

    [Header("Offsets")]
    public float MaxYOffset = 1f, MinYOffset = 0f;

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
    /// Internal helper: fetch an inactive object from a given pool.
    /// </summary>
    private GameObject GetFromPool(List<GameObject> pool)
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }

        // If all are active, recycle the first one
        GameObject recycled = pool[0];
        recycled.SetActive(false);
        return recycled;
    }

    /// <summary>
    /// Core spawn logic for particle/FX objects.
    /// </summary>
    private GameObject SpawnFromPoolInternal(List<GameObject> pool, Transform target)
    {
        GameObject effect = GetFromPool(pool);
        if (effect != null)
        {
            // Match sorting order just above target
            SpriteRenderer targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
            if (targetRenderer != null)
            {
                ParticleSystemRenderer psr = effect.GetComponent<ParticleSystemRenderer>();
                if (psr != null)
                    psr.sortingOrder = targetRenderer.sortingOrder + 1;
            }

            // Randomized Y offset
            float t = Mathf.Pow(Random.value, 0.3f);
            float offset = Mathf.Lerp(MinYOffset, MaxYOffset, t);

            Vector3 newPos = target.position;
            newPos.y += offset;

            effect.transform.position = newPos;
            effect.SetActive(true);
        }
        return effect;
    }

    /// <summary>
    /// Core spawn logic for splatter decals.
    /// </summary>
    private GameObject SpawnSplashInternal(List<GameObject> pool, Transform target)
    {
        GameObject splash = GetFromPool(pool);
        if (splash != null)
        {
            Vector3 newPos = target.position;
            newPos.z = 0; // Make sure decal sits flat on world layer
            splash.transform.position = newPos;

            // Slight random scale
            float scale = Random.Range(0.8f, 1.2f);
            splash.transform.localScale = new Vector3(scale, scale, 1f);

            splash.SetActive(true);
        }
        return splash;
    }

    // --- Public wrappers (only expose target parameter) ---
    public GameObject SpawnBlood(Transform target)
    {
        // Spawn blood particle effect
        GameObject fx = SpawnFromPoolInternal(BloodPoolObjects, target);

        // Also spawn splatter decal
        SpawnSplashInternal(BloodSplashObjects, target);

        return fx;
    }

    public GameObject SpawnHuge(Transform target)
    {
        // Spawn blood particle effect
        GameObject fx = SpawnFromPoolInternal(HugePoolObjects, target);

        // Also spawn splatter decal
        SpawnSplashInternal(BloodSplashObjects, target);

        return fx;
    }

    public GameObject SpawnExplosion(Transform target) => SpawnFromPoolInternal(ExplosionPoolObjects, target);

    public void ClearBloodSplashes()
    {
        foreach (GameObject blood in BloodSplashObjects)
        {
            if (blood.activeInHierarchy)
            {
                blood.SetActive(false);
            }
        }
    }

}
