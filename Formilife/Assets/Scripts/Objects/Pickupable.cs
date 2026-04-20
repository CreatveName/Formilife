using UnityEngine;

public class Pickupable : MonoBehaviour, IPickupable
{
    [SerializeField] private Vector2 holdOffset = new Vector2(0f, 0.5f);
    [SerializeField] private float weight = 1f;

    private Rigidbody2D rb;
    private bool isHeld;

    public float Weight => weight;
    public bool CanBePickedUp => !isHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnPickup(Transform holder)
    {
        isHeld = true;

        if (rb != null) rb.simulated = false;

        transform.SetParent(holder);
        transform.localPosition = holdOffset;
        transform.localRotation = Quaternion.identity;
    }

    public void OnDrop()
    {
        isHeld = false;

        transform.SetParent(null);

        if (rb != null) rb.simulated = true;
    }
}