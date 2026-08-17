using UnityEngine;
using DG.Tweening;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    AudioSource audioSource;

    [Header("BG Music")]
    [SerializeField] private AudioSource bgMusic;
    [SerializeField] private float fadeDuration = 0.5f;

    private Tween bgTween;

    private void Start()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        PlayBG(true);
    }

    public static void PlayAudio(AudioClip clip)
    {
        instance.audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Stops all currently playing SFX on the main AudioSource.
    /// Call this when transitioning to game over or title screen
    /// to ensure gameplay sounds don't bleed into the end sequence.
    /// </summary>
    public static void StopAllSFX()
    {
        if (instance != null && instance.audioSource != null)
        {
            instance.audioSource.Stop();
        }
    }
    public static void PlayBG(AudioClip clip)
    {
        if (instance.audioSource.clip != clip)
        {
            instance.audioSource.clip = clip;
            instance.audioSource.Play();
        }
    }
    public static void PlayBG(bool shouldPlay,float volume=1)
    {
        if (instance == null || instance.bgMusic == null)
        {
            Debug.LogError("BG Music AudioSource is not assigned.");
            return;
        }

        var source = instance.bgMusic;

        instance.bgTween?.Kill(); // prevent stacked fades

        if (shouldPlay)
        {
            if (!source.isPlaying)
                source.Play();

            instance.bgTween = source
                .DOFade(volume, instance.fadeDuration)
                .SetEase(Ease.OutQuad);
        }
        else
        {
            instance.bgTween = source
                .DOFade(0f, instance.fadeDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => source.Stop());
        }
    }
}
