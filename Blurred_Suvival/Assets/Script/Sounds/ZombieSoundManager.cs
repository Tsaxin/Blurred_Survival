using UnityEngine;
using System.Collections.Generic;

public class ZombieSoundManager : MonoBehaviour
{
    public static ZombieSoundManager Instance;

    [Header("Zombie Sounds")]
    public List<AudioClip> idleClips;
    public int maxSimultaneous = 3;
    public float minDelay = 2f;
    public float maxDelay = 6f;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
    public float idleBaseVolume = 0.3f;

    [Header("Audio Pool")]
    public int audioSourcePoolSize = 5;

    [Header("Battle Settings")]

    private List<ZombieAIBase> activeZombies;  // Reference to the live enemy list
    private List<AudioSource> audioPool = new List<AudioSource>();
    private float timer;
    private bool isActive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // Create audio source pool
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 1f; // 3D sound
            src.playOnAwake = false;
            audioPool.Add(src);
        }
    }

    void Update()
    {
        if (!isActive || activeZombies == null) return;

        // Remove null entries (destroyed zombies) in-place
        activeZombies.RemoveAll(z => z == null);

        // Filter out humans (CharacterController attached & enabled)
        List<ZombieAIBase> validZombies = activeZombies.FindAll(z =>
        {
            var cc = z.GetComponent<CharacterController>();
            return cc == null || !cc.enabled; // true = valid zombie
        });

        // Stop automatically if no valid zombies remain
        if (validZombies.Count == 0)
        {
            StopZombieSound();
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            TryPlayRandomZombieGroan(validZombies);
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        timer = Random.Range(minDelay, maxDelay);
    }

    private void TryPlayRandomZombieGroan(List<ZombieAIBase> validZombies)
    {
        if (idleClips.Count == 0 || validZombies.Count == 0) return;

        // Count how many are already playing
        int playing = 0;
        foreach (var src in audioPool)
            if (src.isPlaying) playing++;
        if (playing >= maxSimultaneous) return;

        // Pick a random zombie
        ZombieAIBase chosen = validZombies[Random.Range(0, validZombies.Count)];
        if (chosen == null) return;

        // Find a free audio source
        AudioSource source = audioPool.Find(s => !s.isPlaying);
        if (source == null) return;

        // Pick a random idle clip
        AudioClip clip = idleClips[Random.Range(0, idleClips.Count)];

        // Configure audio source for 2D
        source.clip = clip;
        source.spatialBlend = 0f; // 2D audio
        source.pitch = Random.Range(minPitch, maxPitch);
        source.volume = idleBaseVolume * Random.Range(0.8f, 1f);

        // Play the sound
        source.Play();
    }

    /// <summary>
    /// Call this when battle starts, pass the reference to the live enemy list.
    /// </summary>
    public void StartZombieSound(List<ZombieAIBase> zombies)
    {
        activeZombies = zombies; // Keep reference for dynamic updates
        isActive = true;
        ResetTimer();
    }

    /// <summary>
    /// Call this when battle ends.
    /// </summary>
    public void StopZombieSound()
    {
        isActive = false;
        if (activeZombies != null) activeZombies.Clear();

        // Stop all sounds immediately
        foreach (var src in audioPool)
        {
            src.Stop();
        }
    }
}
