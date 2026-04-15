using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCStats", menuName = "Ant Game/NPC Stats")]
public class NPCStatsSO : ScriptableObject
{
    [Header("Identity")]
    public string antName;
    public AntRole role;

    [Header("Movement")]
    public float moveSpeed;
    public float alarmedSpeedMultiplier = 1.5f;

    [Header("Health")]
    public float maxHealth;
    public float attackDamage;
    public float defense;

    [Header("Personal Needs")]
    public float maxHunger;
    public float maxThirst;
    public float maxSleep;
    public float hungerDecayRate;
    public float thirstDecayRate;
    public float sleepDecayRate;

    [Header("Carrying")]
    public float carryCapacity; // i think this is what he wants idk
    /*
    [Header("Pathfinding")]
    public float pathWanderRadius;
    public float pathPreferenceWeight; // how strongly it prefers known paths
    */

    [Header("Threat Response")]
    public ThreatResponse threatResponse;
    public float alertRadius;
}

public enum AntRole
{
    Worker,
    Soldier,
    Alate
}

public enum ThreatResponse
{
    Fighter,
    Panicker,
    Alerter
}