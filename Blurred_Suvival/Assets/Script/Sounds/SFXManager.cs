using UnityEngine;
using System.Collections.Generic;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Attack Sounds")]
    public List<AudioClip> meleeSlashClips;  // assign in Inspector
    public List<AudioClip> missClips;
    public List<AudioClip> FootStepClips;

    public float baseVolume = 1f;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // Create dedicated AudioSource for one-shot SFX
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2D sound
    }

    /// <summary>
    /// Plays a random melee slash sound.
    /// Call this whenever a melee attack happens.
    /// </summary>
    public void PlayDamageSFX()
    {
        PlaySFX(meleeSlashClips);
    }

    public void PlayMissSound()
    {
        PlaySFX(missClips);
    }

    public void PlayFootStep()
    {
        PlaySFX(FootStepClips);
    }

    public void PlaySFX(List<AudioClip> clips)
    {
        if (clips.Count == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Count)];

        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        sfxSource.volume = baseVolume * Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(clip);
    }
}
