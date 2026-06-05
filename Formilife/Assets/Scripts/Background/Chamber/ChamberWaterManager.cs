using System.Collections.Generic;
using UnityEngine;

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
    private Collider2D[] playerColliders;
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

        droplets.RemoveAll(d => d == null);

        humidChambers.RemoveAll(c => c == null);
        if (humidChambers.Count == 0)
        {
            RefreshHumidChambers();
            if (humidChambers.Count == 0) return;
        }

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

        if (droplet.TryGetComponent(out DrinkEffect drink))
            drink.countsTowardFormicaryWater = false;

        IgnorePlayerCollision(droplet);

        return droplet;
    }

    private void IgnorePlayerCollision(GameObject droplet)
    {
        EnsurePlayerColliders();
        if (playerColliders == null) return;

        Collider2D[] dropletColliders = droplet.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D dc in dropletColliders)
        {
            if (dc == null) continue;
            foreach (Collider2D pc in playerColliders)
            {
                if (pc != null) Physics2D.IgnoreCollision(dc, pc, true);
            }
        }
    }

    private void EnsurePlayerColliders()
    {
        if (playerColliders != null && playerColliders.Length > 0 && playerColliders[0] != null) return;

        PlayerAntMovement player = FindFirstObjectByType<PlayerAntMovement>(FindObjectsInactive.Include);
        playerColliders = player != null ? player.GetComponentsInChildren<Collider2D>() : null;
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

            if (!col.OverlapPoint(candidate)) continue;

            if (Physics2D.OverlapCircle(candidate, wallClearance, wallMask) != null) continue;

            point = new Vector3(candidate.x, candidate.y, chamber.transform.position.z);
            return true;
        }

        return false;
    }
}
