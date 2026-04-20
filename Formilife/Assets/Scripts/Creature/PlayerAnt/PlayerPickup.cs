using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private float pickupRange = 1f;
    [SerializeField] private Transform holdPoint;

    private IPickupable heldItem;
    public IPickupable HeldItem => heldItem;

    public float CurrentCarryWeight
    {
        get
        {
            return heldItem != null ? heldItem.Weight : 0f;
        }
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (heldItem != null)
            {
                Drop();
            }
            else
            {
                TryFindPickup();
            }
        }
    }

    private void TryFindPickup()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IPickupable pickup) && pickup.CanBePickedUp)
            {
                heldItem = pickup;
                pickup.OnPickup(holdPoint);
                break;
            }
        }
    }

    private void Drop()
    {
        heldItem.OnDrop();
        heldItem = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}