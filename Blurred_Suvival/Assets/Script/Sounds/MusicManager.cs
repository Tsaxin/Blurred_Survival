using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    public float fadeDuration = 1.5f; // how long fades take
    private Coroutine currentRoutine;

    public AudioClip AmbientClip, CombatClip, DramaticClip,HoverAudio;

    public AudioSource audioSource, UIAudioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        PlayMusic(DramaticClip);
    }

    /// <summary>
    /// Plays a new track with fade out/in, at the given max volume.
    /// </summary>
    public void PlayMusic(AudioClip newClip, float maxVolume = 0.3f)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeMusic(newClip, maxVolume));
    }

    private IEnumerator FadeMusic(AudioClip newClip, float maxVolume)
    {
        float startVolume = audioSource.volume;

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = 0;
        audioSource.Stop();

        // Switch track
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in up to maxVolume
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, maxVolume, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = maxVolume;

        currentRoutine = null;
    }

    public void PlayHoverSound()
    {
        UIAudioSource.clip = HoverAudio;
        UIAudioSource.Play();
    }
}
