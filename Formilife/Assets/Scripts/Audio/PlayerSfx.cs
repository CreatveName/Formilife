using UnityEngine;

// One-shot player action sound effects (pick up, drop, recruit, dismiss).
// Lives on the player so all action audio is configured in one place; other
// player components grab this via GetComponent and call the Play* methods.
[RequireComponent(typeof(AudioSource))]
public class PlayerSfx : MonoBehaviour
{
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioClip recruitSound;
    [SerializeField] private AudioClip dismissSound;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
    }

    public void PlayPickup() => Play(pickupSound);
    public void PlayDrop() => Play(dropSound);
    public void PlayRecruit() => Play(recruitSound);
    public void PlayDismiss() => Play(dismissSound);

    private void Play(AudioClip clip)
    {
        if (clip != null)
            source.PlayOneShot(clip, volume);
    }
}
