using UnityEngine;
using UnityEngine.InputSystem;

// Attach to the Queen ant. When the player is within interactRange and presses
// E, the Queen replays the current quest's explanation through QueenDialogue.
public class QueenInteract : MonoBehaviour
{
    [Tooltip("Distance (world units) within which the player can press E to talk.")]
    [SerializeField] private float interactRange = 2.5f;

    [Tooltip("Key the player presses to talk. Defaults to E (same key used for eating).")]
    [SerializeField] private Key talkKey = Key.E;

    [Tooltip("Optional explicit player reference. If left empty, the script finds the PlayerAntMovement in the scene at startup.")]
    [SerializeField] private Transform player;

    private void Start()
    {
        if (player == null)
        {
            PlayerAntMovement pam = FindFirstObjectByType<PlayerAntMovement>();
            if (pam != null) player = pam.transform;
        }
    }

    private void Update()
    {
        if (!StartMenu.GameStarted || PauseMenu.IsPaused) return;
        if (player == null || QueenDialogue.Instance == null) return;

        Keyboard kb = Keyboard.current;
        if (kb == null || !kb[talkKey].wasPressedThisFrame) return;

        float sqr = (player.position - transform.position).sqrMagnitude;
        if (sqr <= interactRange * interactRange)
        {
            QueenDialogue.Instance.ReplayCurrentTask();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.9f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
