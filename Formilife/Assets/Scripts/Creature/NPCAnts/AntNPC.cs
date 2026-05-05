using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AntNPC : MonoBehaviour
{
    private enum AntState
    {
        Idle,
        WanderingInPheromone,
        GoingToSeed,
        GoingToFoodStorage
    }

    [Header("Definition")]
    [SerializeField] private AntDefinition antDefinition;

    [Header("Pheromone Work")]
    [SerializeField] private float arriveDistance = 0.25f;
    [SerializeField] private float pickupDistance = 0.25f;

    [Header("Debug")]
    [SerializeField] private AntState currentState;

    private NavMeshAgent agent;
    private AntPerception perception;
    private NPCAntPickup pickup;

    private Transform currentTarget;
    private float idleTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<AntPerception>();
        pickup = GetComponent<NPCAntPickup>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        if (antDefinition == null)
        {
            Debug.LogError($"{name} is missing an AntDefinition.", this);
            enabled = false;
            return;
        }

        agent.speed = antDefinition.moveSpeed;
        BeginIdle();
    }

    private void Update()
    {
        if (!CanUseAgent())
            return;

        switch (currentState)
        {
            case AntState.Idle:
                HandleIdle();
                break;

            case AntState.WanderingInPheromone:
                HandleWanderingInPheromone();
                break;

            case AntState.GoingToSeed:
                HandleGoingToSeed();
                break;

            case AntState.GoingToFoodStorage:
                HandleGoingToFoodStorage();
                break;
        }
    }

    private void HandleIdle()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer > 0f)
            return;

        if (PheromoneManager.Instance == null || !PheromoneManager.Instance.HasAnyTrail())
        {
            BeginIdle();
            return;
        }

        // If holding item, find storage
        if (pickup != null && pickup.IsHoldingSomething)
        {
            Debug.Log($"{name} is holding something, looking for food storage.");
            TryGoToFoodStorage();
            return;
        }

        // If not holding, look for seed
        Transform seed = perception.GetClosestSeedInsidePheromone();

        if (seed != null)
        {
            Debug.Log($"{name} found seed: {seed.name}");
            currentTarget = seed;
            agent.SetDestination(currentTarget.position);
            currentState = AntState.GoingToSeed;
            return;
        }

        // If nothing useful found, wander
        Debug.Log($"{name} found no seed, wandering in pheromone.");
        WanderToRandomPheromonePoint();
    }

    private void HandleWanderingInPheromone()
    {
        if (PheromoneManager.Instance == null || !PheromoneManager.Instance.HasAnyTrail())
        {
            BeginIdle();
            return;
        }

        Transform seed = perception.GetClosestSeedInsidePheromone();

        if (seed != null && pickup != null && !pickup.IsHoldingSomething)
        {
            currentTarget = seed;
            agent.SetDestination(currentTarget.position);
            currentState = AntState.GoingToSeed;
            return;
        }

        if (HasArrived())
        {
            BeginIdle();
        }
    }

    private void HandleGoingToSeed()
    {
        if (currentTarget == null)
        {
            BeginIdle();
            return;
        }

        if (!PheromoneManager.Instance.IsInsidePheromone(currentTarget.position))
        {
            currentTarget = null;
            BeginIdle();
            return;
        }

        agent.SetDestination(currentTarget.position);

        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist <= pickupDistance)
        {
            IPickupable item = currentTarget.GetComponent<IPickupable>();

            if (item != null && pickup.TryPickUp(item))
            {
                currentTarget = null;
                TryGoToFoodStorage();
                return;
            }

            currentTarget = null;
            BeginIdle();
        }
    }

    private void TryGoToFoodStorage()
    {
        Transform storage = perception.GetClosestFoodStorageInsidePheromone();

        if (storage == null)
        {
            BeginIdle();
            return;
        }

        currentTarget = storage;
        agent.SetDestination(currentTarget.position);
        currentState = AntState.GoingToFoodStorage;
    }

    private void HandleGoingToFoodStorage()
    {
        if (currentTarget == null)
        {
            BeginIdle();
            return;
        }

        if (!PheromoneManager.Instance.IsInsidePheromone(currentTarget.position))
        {
            BeginIdle();
            return;
        }

        agent.SetDestination(currentTarget.position);

        if (HasArrived())
        {
            if (pickup != null && pickup.IsHoldingSomething)
                pickup.Drop();

            currentTarget = null;
            BeginIdle();
        }
    }

    private void WanderToRandomPheromonePoint()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector3 randomPoint = PheromoneManager.Instance.GetRandomPheromonePoint();

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                currentState = AntState.WanderingInPheromone;
                return;
            }
        }

        Debug.LogWarning($"{name} could not find valid NavMesh point inside pheromone.");
        BeginIdle();
    }

    private void BeginIdle()
    {
        idleTimer = Random.Range(antDefinition.minIdleTime, antDefinition.maxIdleTime);
        currentTarget = null;
        currentState = AntState.Idle;
    }

    private bool HasArrived()
    {
        if (!CanUseAgent())
            return false;

        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= arriveDistance;
    }

    private bool CanUseAgent()
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }
}