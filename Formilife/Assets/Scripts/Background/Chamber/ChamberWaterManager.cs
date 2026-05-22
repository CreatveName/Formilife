using System.Collections.Generic;
using UnityEngine;

// Keeps a fixed number of water droplets alive across ALL humid chambers on the
// map (4 total, not per chamber). Droplets are tracked by reference, so when one
// is consumed (DrinkEffect destroys it) a replacement spawns in a random humid
// chamber. Self-bootstrapping: a single instance is created automatically after
// the scene loads, so nothing needs to be placed in the scene by hand.
public class ChamberWaterManager : MonoBehaviour
{
    public static ChamberWaterManager Instance { get; private set; }

    [Tooltip("Total number of water droplets to keep alive across all humid chambers.")]
    [SerializeField] private int totalCount = 4;

    [Tooltip("Keeps droplets away from the very edge of a chamber (world units).")]
    [SerializeField] private float edgePadding = 0.3f;

    [Tooltip("How often (seconds) to check for and replace consumed droplets.")]
    [SerializeField] private float refillInterval = 0.5f;

    [Tooltip("Reject spawn points within this distance of a wall, so droplets " +
             "never end up embedded in the formicary walls (unreachable).")]
    [SerializeField] private float wallClearance = 0.5f;

    private GameObject dropletPrefab;
    private int wallMask;
    private readonly List<Chamber> humidChambers = new List<Chamber>();
    private readonly List<GameObject> droplets = new List<GameObject>();
    private float refillTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("ChamberWaterManager");
        go.AddComponent<ChamberWaterManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        wallMask = LayerMask.GetMask("WALL");

        dropletPrefab = Resources.Load<GameObject>("Water Droplet");
        if (dropletPrefab == null)
            Debug.LogError("[ChamberWaterManager] Could not load 'Water Droplet' prefab from a Resources folder.");
    }

    private void Start()
    {
        RefreshHumidChambers();
        Refill();
    }

    private void Update()
    {
        refillTimer += Time.deltaTime;
        if (refillTimer < refillInterval) return;
        refillTimer = 0f;

        Refill();
    }

    private void RefreshHumidChambers()
    {
        humidChambers.Clear();
        Chamber[] all = FindObjectsByType<Chamber>(FindObjectsInactive.Exclude);
        foreach (Chamber c in all)
        {
            if (c != null && c.humidity == HumidityLevel.Humid)
                humidChambers.Add(c);
        }
    }

    private void Refill()
    {
        if (dropletPrefab == null) return;

        // Forget droplets that were consumed/destroyed.
        droplets.RemoveAll(d => d == null);

        // Re-scan if our chamber list went stale (e.g. chambers spawned late).
        humidChambers.RemoveAll(c => c == null);
        if (humidChambers.Count == 0)
        {
            RefreshHumidChambers();
            if (humidChambers.Count == 0) return;
        }

        // Bounded attempts so we never loop forever if no valid point is found.
        int attempts = (totalCount - droplets.Count) * 5;
        while (droplets.Count < totalCount && attempts-- > 0)
        {
            Chamber chamber = humidChambers[Random.Range(0, humidChambers.Count)];
            if (chamber == null) { humidChambers.Remove(chamber); continue; }

            if (TryGetPointInChamber(chamber, out Vector3 pos))
                droplets.Add(SpawnDroplet(pos));
        }
    }

    private GameObject SpawnDroplet(Vector3 position)
    {
        GameObject droplet = Instantiate(dropletPrefab, position, Quaternion.identity);

        // These droplets are managed here, so they must not feed into
        // Formicary's separate global water count when drunk.
        if (droplet.TryGetComponent(out DrinkEffect drink))
            drink.countsTowardFormicaryWater = false;

        return droplet;
    }

    private bool TryGetPointInChamber(Chamber chamber, out Vector3 point)
    {
        point = default;

        Collider2D col = chamber.GetComponent<Collider2D>();
        if (col == null) return false;

        Bounds b = col.bounds;
        float minX = b.min.x + edgePadding;
        float maxX = b.max.x - edgePadding;
        float minY = b.min.y + edgePadding;
        float maxY = b.max.y - edgePadding;

        for (int i = 0; i < 10; i++)
        {
            Vector2 candidate = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));

            // Must be inside the chamber...
            if (!col.OverlapPoint(candidate)) continue;

            // ...and clear of the formicary walls, or the player can't reach it.
            if (Physics2D.OverlapCircle(candidate, wallClearance, wallMask) != null) continue;

            point = new Vector3(candidate.x, candidate.y, chamber.transform.position.z);
            return true;
        }

        return false;
    }
}
