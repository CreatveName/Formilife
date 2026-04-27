using UnityEngine;

public class PheromoneRouteCreator : MonoBehaviour
{
    [SerializeField] private LayerMask zoneLayer;
    [SerializeField] private PheromoneRoute routePrefab;

    private PheromoneZone selectedStart;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectZone();
        }
    }

    private void TrySelectZone()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld, zoneLayer);

        if (hit == null) return;

        PheromoneZone zone = hit.GetComponent<PheromoneZone>();
        if (zone == null) return;

        if (selectedStart == null)
        {
            selectedStart = zone;
            Debug.Log("Selected start zone.");
            return;
        }

        if (zone == selectedStart)
        {
            Debug.Log("Cannot route a zone to itself.");
            return;
        }

        PheromoneRoute route = Instantiate(routePrefab);
        route.startZone = selectedStart;
        route.targetZone = zone;

        PheromoneRouteManager.Instance.SetRoute(route);

        Debug.Log("Created pheromone route.");

        selectedStart = null;
    }
}