using UnityEngine;
using UnityEngine.InputSystem;

public class Pickupable : MonoBehaviour
{
    [Header("pickup")]
    [SerializeField] private float pickupRange = 1f;
    [SerializeField] private Vector2 holdOffset = new Vector2(0f, 0.5f);
    [SerializeField] private float weight = 1f;

    private static Pickupable heldPickupable;
    private static int lastActionFrame = -1;

    public static float HeldWeight => heldPickupable != null ? heldPickupable.weight : 0f;

    private Transform player;
    private Rigidbody2D rb;
    private bool isHeld;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void Update() {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || player == null) return;
        if (!keyboard.spaceKey.wasPressedThisFrame) return;
        if (lastActionFrame == Time.frameCount) return;
        if (isHeld) {
            Drop();
        }
        else if (heldPickupable == null && Vector2.Distance(transform.position, player.position) <= pickupRange) {
            PickUp();
        }
    }

    private void PickUp()
    {
        isHeld = true;
        heldPickupable = this;
        lastActionFrame = Time.frameCount;
        if (rb != null) rb.simulated = false;
        transform.SetParent(player);
        transform.localPosition = holdOffset;
        transform.localRotation = Quaternion.identity;
    }

    private void Drop()
    {
        isHeld = false;
        heldPickupable = null;
        lastActionFrame = Time.frameCount;

        transform.SetParent(null);

        if (rb != null) rb.simulated = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
