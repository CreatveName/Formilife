using UnityEngine;

public class PheromoneZone : MonoBehaviour
{
    [SerializeField] private Transform dropPoint;

    public Vector2 DropPosition => dropPoint != null ? dropPoint.position : transform.position;
}