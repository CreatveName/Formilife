using UnityEngine;

public class NPCAntPickup : MonoBehaviour
{
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

    public bool TryPickUp(IPickupable item)
    {
        if (heldItem != null || !item.CanBePickedUp) return false;

        heldItem = item;
        item.OnPickup(holdPoint);

        // NEW: auto-clear if item was consumed instantly
        if (heldItem == null || ((MonoBehaviour)heldItem) == null)
        {
            heldItem = null;
        }

        return heldItem != null;
    }

    public void Drop()
    {
        if (heldItem == null) return;

        heldItem.OnDrop();
        heldItem = null;
    }
}