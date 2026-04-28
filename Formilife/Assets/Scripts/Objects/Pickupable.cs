using UnityEngine;

public class Pickupable : MonoBehaviour, IPickupable
{
    public float Weight => weight;
    [SerializeField] private float weight = 0f;

    public bool CanBePickedUp => true;

    public GameObject GameObject => gameObject;

    private Rigidbody2D rb;

    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnPickup(Transform holder)
    {
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;

        if (rb != null)
        {
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void OnDrop()
    {
        transform.SetParent(null);

        if (rb != null)
        {
            rb.simulated = true;
        }
    }
}