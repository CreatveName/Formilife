using UnityEngine;
using UnityEngine.AI;

public class AntEgg : MonoBehaviour
{
    [Header("Hatching")]
    [SerializeField] private float hatchTime = 10f;

    [Header("Ant Prefabs")]
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private GameObject soldierPrefab;

    [Header("Spawn Chances")]
    [SerializeField, Range(0f, 100f)] private float workerChance = 75f;

    private float hatchTimer;

    private void Start()
    {
        hatchTimer = hatchTime;
    }

    private void Update()
    {
        hatchTimer -= Time.deltaTime;

        if (hatchTimer <= 0f)
        {
            Hatch();
        }
    }

    private void Hatch()
    {
        GameObject prefabToSpawn = ChooseAntPrefab();

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"{name} has no ant prefab assigned.", this);
            Destroy(gameObject);
            return;
        }

        Vector3 spawnPos = transform.position;

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
        }

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        Destroy(gameObject);
    }

    private GameObject ChooseAntPrefab()
    {
        float roll = Random.Range(0f, 100f);

        if (roll <= workerChance)
        {
            return workerPrefab;
        }

        return soldierPrefab;
    }
}