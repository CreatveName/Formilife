using UnityEngine;

public class AntPerception : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask seedLayer;
    [SerializeField] private LayerMask foodStorageLayer;

    public Transform GetClosestSeedInsidePheromone()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, seedLayer);

        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (!PheromoneManager.Instance.IsInsidePheromone(hit.transform.position))
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

    public Transform GetClosestFoodStorageInsidePheromone()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 999f, foodStorageLayer);

        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (!PheromoneManager.Instance.IsInsidePheromone(hit.transform.position))
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
}