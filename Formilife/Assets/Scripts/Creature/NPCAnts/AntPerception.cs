using System.Collections.Generic;
using UnityEngine;

public class AntPerception : MonoBehaviour
{
    [Header("Sensing")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask foodLayer;

    public Transform GetClosestFood()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, foodLayer);

        if (hits.Length == 0)
            return null;

        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
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