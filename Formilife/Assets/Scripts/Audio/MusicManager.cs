using UnityEngine;

// Plays looping background music. Survives scene loads so the track keeps
// playing from the menu into gameplay. Put one of these in your first scene.
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip track;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float fadeInSeconds = 1.5f;

    [Header("Eating SFX")]
    [SerializeField] private AudioClip eatSound;
    [Range(0f, 1f)]
    [SerializeField] private float eatVolume = 1f;

    [Header("Drinking SFX")]
    [SerializeField] private AudioClip drinkSound;
    [Range(0f, 1f)]
    [SerializeField] private float drinkVolume = 1f;

    public static MusicManager Instance { get; private set; }
    private AudioSource source;

    private void Awake()
    {
        // Only one music player should exist; destroy duplicates from scene reloads.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        source.clip = track;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = fadeInSeconds > 0f ? 0f : volume;
    }

    private void Start()
    {
        if (playOnStart) Play();
    }

    public void Play()
    {
        if (track == null || source.isPlaying) return;
        source.Play();
    }

    public void SetVolume(float v) => volume = Mathf.Clamp01(v);

    public void Stop() => source.Stop();

    public void PlayEat(Vector3 position)
    {
        if (eatSound != null)
            source.PlayOneShot(eatSound, eatVolume);
    }

    public void PlayDrink(Vector3 position)
    {
        if (drinkSound != null)
            source.PlayOneShot(drinkSound, drinkVolume);
    }

    private void Update()
    {
        // Fade up to the target volume on start.
        if (fadeInSeconds > 0f && source.isPlaying && source.volume < volume)
        {
            source.volume = Mathf.MoveTowards(
                source.volume, volume, (volume / fadeInSeconds) * Time.unscaledDeltaTime);
        }
    }
}
