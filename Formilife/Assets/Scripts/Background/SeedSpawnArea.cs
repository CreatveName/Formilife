using System.Collections.Generic;
using UnityEngine;

// Maintains a random number of seeds (between min and max) scattered inside
// a rectangular area centered on this transform. When seeds are picked up or
// destroyed, the area periodically tops itself back up to a fresh random
// target count.
public class SeedSpawnArea : MonoBehaviour
{
    [Header("Seeds")]
    [Tooltip("Seed prefabs to choose from at random when spawning.")]
    [SerializeField] private List<GameObject> seedPrefabs = new List<GameObject>();

    [Header("Count")]
    [SerializeField] private int minSeeds = 4;
    [SerializeField] private int maxSeeds = 7;

    [Header("Area (local size around this transform)")]
    [Tooltip("Half-extents of the spawn rectangle on the XY plane.")]
    [SerializeField] private Vector2 areaSize = new Vector2(6f, 6f);
    [Tooltip("Local Z offset for spawned seeds (depth).")]
    [SerializeField] private float spawnZ = 0f;

    [Header("Behavior")]
    [Tooltip("How often (seconds) to check whether more seeds need to be spawned.")]
    [SerializeField] private float topUpInterval = 2f;
    [Tooltip("Optional parent for spawned seeds; defaults to this transform.")]
    [SerializeField] private Transform spawnParent;

    [Header("Rendering")]
    [Tooltip("If set, overrides the SpriteRenderer.sortingLayerName of spawned seeds.")]
    [SerializeField] private string sortingLayerName = "";
    [Tooltip("Sorting order applied to each spawned seed's SpriteRenderer so they draw above the background.")]
    [SerializeField] private int sortingOrder = 50;

    private readonly List<GameObject> alive = new List<GameObject>();
    private int targetCount;
    private float nextCheckTime;

    private void Start()
    {
        targetCount = RandomTarget();
        TopUp();
    }

    private void Update()
    {
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + Mathf.Max(0.1f, topUpInterval);

        PruneDead();
        if (alive.Count < targetCount)
        {
            TopUp();
        }
        else if (alive.Count >= targetCount)
        {
            // Refresh the target so the next dip lands at a new random count.
            targetCount = RandomTarget();
        }
    }

    private int RandomTarget()
    {
        int lo = Mathf.Min(minSeeds, maxSeeds);
        int hi = Mathf.Max(minSeeds, maxSeeds);
        return Random.Range(lo, hi + 1);
    }

    private void PruneDead()
    {
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            GameObject s = alive[i];
            // Treat destroyed, deactivated, or reparented (picked up) seeds as gone.
            if (s == null || !s.activeInHierarchy || s.transform.parent != (spawnParent != null ? spawnParent : transform))
                alive.RemoveAt(i);
        }
    }

    private void TopUp()
    {
        if (seedPrefabs == null || seedPrefabs.Count == 0) return;
        Transform parent = spawnParent != null ? spawnParent : transform;

        while (alive.Count < targetCount)
        {
            GameObject prefab = seedPrefabs[Random.Range(0, seedPrefabs.Count)];
            if (prefab == null) continue;

            Vector3 localPos = new Vector3(
                Random.Range(-areaSize.x, areaSize.x),
                Random.Range(-areaSize.y, areaSize.y),
                spawnZ);
            Vector3 worldPos = transform.TransformPoint(localPos);

            GameObject seed = Instantiate(prefab, worldPos, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), parent);

            SpriteRenderer sr = seed.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                if (!string.IsNullOrEmpty(sortingLayerName)) sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;
            }

            alive.Add(seed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.6f);
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(0f, 0f, spawnZ), new Vector3(areaSize.x * 2f, areaSize.y * 2f, 0.1f));
        Gizmos.matrix = prev;
    }
}
