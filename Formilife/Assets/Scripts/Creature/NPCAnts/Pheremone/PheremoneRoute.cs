using UnityEngine;

public class PheromoneRoute : MonoBehaviour
{
    public PheromoneZone startZone;
    public PheromoneZone targetZone;

    public Vector2 StartPosition => startZone.DropPosition;
    public Vector2 TargetPosition => targetZone.DropPosition;

    public bool IsValid => startZone != null && targetZone != null;
}