using System.Collections.Generic;
using UnityEngine;

public class PheromoneManager : MonoBehaviour
{
    public static PheromoneManager Instance { get; private set; }

    private readonly List<PheromoneTrail> trails = new List<PheromoneTrail>();
    private readonly List<Chamber> chambers = new List<Chamber>();

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
    }

    public void RegisterChamber(Chamber chamber)
    {
        if (!chambers.Contains(chamber))
            chambers.Add(chamber);
    }

    public void UnregisterChamber(Chamber chamber)
    {
        chambers.Remove(chamber);
    }

    public bool IsInsidePheromone(Vector3 position)
    {
        foreach (PheromoneTrail trail in trails)
        {
            if (trail != null && trail.ContainsPoint(position))
                return true;
        }

        foreach (Chamber chamber in chambers)
        {
            if (chamber != null &&
                chamber.current == ChamberRole.FoodStorage &&
                chamber.ContainsPoint(position))
            {
                return true;
            }
        }

        return false;
    }
    public bool IsInsideThroneRoom(Vector3 position)
    {
        foreach (Chamber chamber in chambers)
        {
            if (chamber != null &&
                chamber.current == ChamberRole.ThroneRoom &&
                chamber.ContainsPoint(position))
            {
                return true;
            }
        }

        return false;
    }

    public Transform GetClosestFoodStorage(Vector3 position)
    {
        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (Chamber chamber in chambers)
        {
            if (chamber == null)
                continue;

            if (chamber.current != ChamberRole.FoodStorage)
                continue;

            float dist = Vector2.Distance(position, chamber.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = chamber.transform;
            }
        }

        return closest;
    }

    public bool IsInsideFoodStorage(Vector3 position)
    {
        foreach (Chamber chamber in chambers)
        {
            if (chamber != null &&
                chamber.current == ChamberRole.FoodStorage &&
                chamber.ContainsPoint(position))
            {
                return true;
            }
        }

        return false;
    }

    public Vector3 GetRandomPheromonePoint()
    {
        if (trails.Count > 0)
        {
            PheromoneTrail trail = trails[Random.Range(0, trails.Count)];
            return trail.GetRandomPoint();
        }

        List<Chamber> foodChambers = new List<Chamber>();

        foreach (Chamber chamber in chambers)
        {
            if (chamber != null && chamber.current == ChamberRole.FoodStorage)
                foodChambers.Add(chamber);
        }

        if (foodChambers.Count > 0)
            return foodChambers[Random.Range(0, foodChambers.Count)].transform.position;

        return Vector3.zero;
    }

    public bool HasAnyTrail()
    {
        if (trails.Count > 0)
            return true;

        foreach (Chamber chamber in chambers)
        {
            if (chamber != null && chamber.current == ChamberRole.FoodStorage)
                return true;
        }

        return false;
    }
}