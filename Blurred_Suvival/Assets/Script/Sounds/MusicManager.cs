using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    public float fadeDuration = 1.5f; // how long fades take
    private Coroutine currentRoutine;

    public AudioClip AmbientClip, CombatClip, DramaticClip, HoverAudio, SelectClip,EquipSound,InventorySound,PickUpSound;

    public AudioSource audioSource, UIAudioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        PlayDramaticMusic();
    }

    /// <summary>
    /// Plays a new track with fade out/in, at the given max volume.
    /// </summary>
    public void PlayDramaticMusic()
    {
        PlayMusic(DramaticClip);
    }

    public void PlayAmbientMusic()
    {
        PlayMusic(AmbientClip);
    }

    public void PlayBattleMusic()
    {
        PlayMusic(CombatClip);
    }
    public void PlayMusic(AudioClip newClip, float maxVolume = 1f)
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
        PlayUISound(HoverAudio);
    }

    public void PlaySelectSound()
    {
        PlayUISound(SelectClip);
    }
    public void PlayInventorySound()
    {
        PlayUISound(InventorySound);
    }
    public void PlayEquipSound()
    {
        PlayUISound(EquipSound);
    }
    public void PlayPickUpSound()
    {
        PlayUISound(PickUpSound);
    }

    public void PlayUISound(AudioClip audioClip)
    {
        if (audioClip != null)
        {
            UIAudioSource.clip = audioClip;
            UIAudioSource.Play();
        }
        else Debug.Log("audioClip is missing");
    }
}
