using UnityEngine;

public class PheromoneRouteManager : MonoBehaviour
{
    public static PheromoneRouteManager Instance { get; private set; }

    public PheromoneRoute ActiveRoute { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SetRoute(PheromoneRoute route)
    {
        ActiveRoute = route;
    }
}