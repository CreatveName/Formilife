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

        // auto-clear if item was consumed/destroyed instantly
        if (heldItem != null && heldItem.GameObject == null)
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