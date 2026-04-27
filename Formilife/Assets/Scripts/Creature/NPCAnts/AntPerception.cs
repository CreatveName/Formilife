using UnityEngine;

public class AntPerception : MonoBehaviour
{
    [Header("Sensing")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask pickupLayer;

    public Transform GetClosestPickupable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, pickupLayer);

        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            IPickupable pickupable = hit.GetComponent<IPickupable>();

            if (pickupable == null || !pickupable.CanBePickedUp)
                continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit.transform;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}