using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private float pickupRange = 1f;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private int soldiersRequiredForQueen = 2;
    private PlayerAntRecruiter recruiter;

    private IPickupable heldItem;
    public IPickupable HeldItem => heldItem;

    public Action<IPickupable> OnPickedUpItem;
    public Action OnDroppedItem;

    public float CurrentCarryWeight
    {
        get
        {
            return heldItem != null ? heldItem.Weight : 0f;
        }
    }
    private void Awake()
    {
        recruiter = GetComponent<PlayerAntRecruiter>();
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

                if (food != null && (!food.needsCrack||food.cracked))
                {
                    food.Consume(gameObject);
                    DestroyHeldItem();
                }
            }
            else
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);

                    foreach (var hit in hits)
                    {
                        Debug.Log("i see something");
                        if (hit.TryGetComponent(out DrinkEffect liquid))
                        {
                            liquid.Drink(gameObject);
                            DestroyHeldItem();
                            Destroy(liquid.gameObject);
                        }
                    }
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
                if (!CanPickUpThisItem(pickup))
                    continue;

                heldItem = pickup;
                pickup.OnPickup(holdPoint);

                OnPickedUpItem?.Invoke(heldItem);
                break;
            }
        }
    }
    private bool CanPickUpThisItem(IPickupable pickup)
    {
        AntNPC ant = pickup.GameObject.GetComponentInParent<AntNPC>();

        if (ant == null)
            return true;

        if (ant.GetRole() != AntRole.Queen)
            return true;

        int recruitCount = recruiter != null ? recruiter.GetRecruitCount() : 0;

        Debug.Log($"Trying to pick up queen. Recruits: {recruitCount}/{soldiersRequiredForQueen}");

        if (recruitCount < soldiersRequiredForQueen)
        {
            Debug.Log($"Need at least {soldiersRequiredForQueen} recruited soldiers to move the queen.");
            return false;
        }

        return true;
    }

    public void Drop()
    {
        if (heldItem == null) return;
        heldItem.OnDrop();
        OnDroppedItem?.Invoke();
        heldItem = null;
    }
    public void DestroyHeldItem()
    {
        if (heldItem == null) return;

        heldItem.OnDrop();

        OnDroppedItem?.Invoke();

        Destroy(heldItem.GameObject);

        heldItem = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}