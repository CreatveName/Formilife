using System.Collections.Generic;

public enum ChamberRole
{
    Unassigned,
    FoodStorage,
    Nursery,
    ThroneRoom,
    Landfill
}

public enum HumidityLevel { Dry, Humid, Neutral }
public enum SafetyLevel  { Safe, Dangerous, Neutral }


[System.Serializable]
public class AssignmentRule
{
    public string    requiredItemTag;
    public int       requiredCount;
    public ChamberRole assignedRole; 
}