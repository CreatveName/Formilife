using UnityEngine;

[CreateAssetMenu(fileName = "AntNPCDefinition", menuName = "Ants/NPC Ant Definition")]
public class AntNPCDefinition : ScriptableObject
{
    [Header("Role")]
    public AntRole role = AntRole.Worker;

    [Header("NPC Movement")]
    public float wanderRadius = 3f;
    public float minIdleTime = 0.5f;
    public float maxIdleTime = 2f;

    [Header("NPC Behavior")]
    public bool collectsSeeds = true;
    public bool storesSeeds = true;
    public bool cracksBigSeeds = false;
    public bool patrolsFormicary = false;
    public bool fleesFromThreats = true;
    public bool warnsSoldiers = true;
    public bool laysEggs = false;
}