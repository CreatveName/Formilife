using System.Collections.Generic;
using UnityEngine;

public class PheromoneManager : MonoBehaviour
{
    public static PheromoneManager Instance { get; private set; }

    private readonly List<PheromoneTrail> trails = new List<PheromoneTrail>();
    public List<PheromoneTrail> GetAllTrails()
    {
        return trails;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterTrail(PheromoneTrail trail)
    {
        if (!trails.Contains(trail))
            trails.Add(trail);

        Debug.Log("Trail registered. Total trails: " + trails.Count);
    }

    public bool IsInsidePheromone(Vector3 position)
    {
        foreach (PheromoneTrail trail in trails)
        {
            if (trail != null && trail.ContainsPoint(position))
                return true;
        }

        return false;
    }

    public Vector3 GetRandomPheromonePoint()
    {
        if (trails.Count == 0)
            return Vector3.zero;

        PheromoneTrail trail = trails[Random.Range(0, trails.Count)];
        return trail.GetRandomPoint();
    }

    public bool HasAnyTrail()
    {
        Debug.Log("Checking trails. Count: " + trails.Count);
        return trails.Count > 0;
    }
}