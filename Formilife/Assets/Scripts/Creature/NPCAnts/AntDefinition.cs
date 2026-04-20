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

    // =========================
    // NEEDS CONFIG (NEW)
    // =========================
    [Header("Needs")]
    public float maxHunger = 100f;
    public float hungerDecayRate = 2f;

    public float maxThirst = 100f;
    public float thirstDecayRate = 3f;

    public float maxEnergy = 100f;
    public float energyDecayRate = 1f;

    [Header("Behavior Thresholds")]
    public float lowHungerThreshold = 40f;
    public float lowThirstThreshold = 40f;
    public float tiredThreshold = 30f;
}