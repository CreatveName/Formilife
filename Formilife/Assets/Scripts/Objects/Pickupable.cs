using UnityEngine;

public class Pickupable : MonoBehaviour, IPickupable
{
    public float Weight => weight;
    [SerializeField] private float weight = 0f;

    public bool CanBePickedUp => true;

    public GameObject GameObject => gameObject;

    private Rigidbody2D rb;

    public static double stackCount = 1.0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Pickupable other = collision.gameObject.GetComponent<Pickupable>();
        if (other != null && transform.parent == null && other.transform.parent == null)
        {
            stackCount +=0.5;
            //Destroy(other.gameObject);
            Debug.Log("stack size: " + stackCount);
        }
    }
    public void OnPickup(Transform holder)
    {
        if (stackCount > 1)
        {
            stackCount = 1;
        }
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;

        if (rb != null)
        {
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        Debug.Log("stack size: " + stackCount);
    }

    public void OnDrop()
    {
        transform.SetParent(null);

        if (rb != null)
        {
            rb.simulated = true;
        }
        Debug.Log("stack size: " + stackCount);
    }
}