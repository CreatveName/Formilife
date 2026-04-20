using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private float pickupRange = 1f;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private LayerMask pickupMask;

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
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (heldItem != null)
            {
                GameObject obj = heldItem.GameObject;

                FoodEffect food = obj.GetComponent<FoodEffect>();

                if (food != null)
                {
                    food.Consume(gameObject);
                    DestroyHeldItem();
                }
            }
        }
    }

    private void TryFindPickup()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange, pickupMask);

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
    public void DestroyHeldItem()
    {
        if (heldItem == null) return;

        heldItem.OnDrop();

        Object.Destroy(heldItem.GameObject);

        heldItem = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}