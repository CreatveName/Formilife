using System.Collections.Generic;
using UnityEngine;

public class Chamber: MonoBehaviour
{
    public HumidityLevel humidity = HumidityLevel.Neutral;
    public SafetyLevel safety = SafetyLevel.Safe;

    public ChamberRole current = ChamberRole.Unassigned;
    private Collider2D chamberCollider;

    public List<AssignmentRule> assignmentRules = new();

    private Dictionary<string,int> _itemCounts = new();
    private HashSet<ChamberItem> _trackedItems = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EvaluateRole();

        if (PheromoneManager.Instance != null)
            PheromoneManager.Instance.RegisterChamber(this);
    }

    private void Awake()
    {
        chamberCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (PheromoneManager.Instance != null)
            PheromoneManager.Instance.RegisterChamber(this);
    }

    private void OnDisable()
    {
        if (PheromoneManager.Instance != null)
            PheromoneManager.Instance.UnregisterChamber(this);
    }

    public bool ContainsPoint(Vector3 worldPosition)
    {
        if (chamberCollider == null)
            return false;

        return chamberCollider.OverlapPoint(worldPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ChamberItem item = other.GetComponent<ChamberItem>();
        if (item == null) return;
        if (_trackedItems.Add(item))
        {
            Debug.Log($"Adding to count: {item.name} (tag '{item.itemTag}')", item);
            AddCount(item.itemTag, 1);
            EvaluateRole();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        ChamberItem item = other.GetComponent<ChamberItem>();
        if(item == null) return;
        if (_trackedItems.Remove(item))
        {
            AddCount(item.itemTag, -1);
            EvaluateRole();
        }
    }

    public void EvaluateRole()
    {
        foreach(AssignmentRule rule in assignmentRules)
        {
            _itemCounts.TryGetValue(rule.requiredItemTag, out int count);
            if(count >= rule.requiredCount)
            {
                Debug.Log("setting role");
                SetRole(rule.assignedRole);
                return;
            }
        }
        SetRole(ChamberRole.Unassigned);
    }

    private void SetRole(ChamberRole newRole)
    {
        if (current == newRole) return;

        ChamberRole oldRole = current;

        Debug.Log($"Chamber '{name}' assigned role: {newRole} (was {current})", this);
        current = newRole;

        if (PheromoneManager.Instance != null)
        {
            PheromoneManager.Instance.UnregisterChamber(this);
            PheromoneManager.Instance.RegisterChamber(this);
        }
    }

    private void AddCount(string tag, int delta){
        _itemCounts.TryGetValue(tag, out int current);
        int updated = Mathf.Max(0, current + delta);

        if (updated == 0)
            _itemCounts.Remove(tag);
        else
            _itemCounts[tag] = updated;
    }
    public int GetItemCount(string tag)
    {
        _itemCounts.TryGetValue(tag, out int count);
        return count;
    }
    
}
