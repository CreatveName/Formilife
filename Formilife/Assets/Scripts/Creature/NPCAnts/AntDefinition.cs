using UnityEngine;

[CreateAssetMenu(fileName = "AntDefinition", menuName = "Ants/Ant Definition")]
public class AntDefinition : ScriptableObject
{
    [Header("Identity")]
    public string antName = "Basic Ant";

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float wanderRadius = 3f;

    [Header("Idle")]
    public float minIdleTime = 0.5f;
    public float maxIdleTime = 2f;
}