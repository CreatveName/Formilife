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
        GoingToFoodStorage,
        GoingToFoodToEat,
        GoingToCrackSeed
    }

    [Header("Definition")]
    [SerializeField] private AntDefinition antDefinition;
    [SerializeField] private AntNPCDefinition npcDefinition;

    [Header("Pheromone Work")]
    [SerializeField] private float arriveDistance = 0.25f;
    [SerializeField] private float pickupDistance = 0.25f;

    [Header("Debug")]
    [SerializeField] private AntState currentState;
    [Header("Survival")]
    [SerializeField] private float eatDistance = 0.35f;

    private AntNeeds needs;

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
        needs = GetComponent<AntNeeds>();

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
        if (npcDefinition == null)
        {
            Debug.LogError($"{name} is missing an AntNPCDefinition.", this);
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
    
        if (TryInterruptForHunger())
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
            case AntState.GoingToFoodToEat:
                HandleGoingToFoodToEat();
                break;
            case AntState.GoingToCrackSeed:
                HandleGoingToCrackSeed();
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
        if (npcDefinition.role == AntRole.Queen)
        {
            // Queen does not do worker tasks right now.
            WanderToRandomPheromonePoint();
            return;
        }

        if (npcDefinition.role == AntRole.Soldier)
        {
            if (TryGoToCrackableSeed())
                return;

            WanderToRandomPheromonePoint();
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
        if (!npcDefinition.collectsSeeds)
        {
            WanderToRandomPheromonePoint();
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

        if (npcDefinition.role == AntRole.Soldier)
        {
            if (TryGoToCrackableSeed())
                return;

            if (HasArrived())
                BeginIdle();

            return;
        }

        if (npcDefinition.collectsSeeds)
        {
            Transform seed = perception.GetClosestSeedInsidePheromone();

            if (seed != null && pickup != null && !pickup.IsHoldingSomething)
            {
                currentTarget = seed;
                agent.SetDestination(currentTarget.position);
                currentState = AntState.GoingToSeed;
                return;
            }
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
        idleTimer = Random.Range(npcDefinition.minIdleTime, npcDefinition.maxIdleTime);
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
    private bool TryInterruptForHunger()
    {
        if (needs == null)
            return false;

        if (!needs.IsHungry())
            return false;

        if (currentState == AntState.GoingToFoodToEat)
            return false;

        // Queen should be allowed to eat, but she should NOT drop tasks/items
        // because she usually should not be carrying things anyway.
        if (npcDefinition.role == AntRole.Queen)
        {
            return TryGoToClosestFoodToEat();
        }

        // Soldiers should not constantly abandon patrols the second they are a little hungry.
        // Let them only eat when they are seriously low.
        if (npcDefinition.role == AntRole.Soldier)
        {
            float emergencyHungerLevel = 0.20f; // 20%

            if (needs.GetHungerNormalized() > emergencyHungerLevel)
                return false;

            return TryGoToClosestFoodToEat();
        }

        // Workers can interrupt normally because gathering/eating is their main job.
        if (npcDefinition.role == AntRole.Worker)
        {
            if (pickup != null && pickup.IsHoldingSomething)
            {
                pickup.Drop();
            }

            return TryGoToClosestFoodToEat();
        }

        return false;
    }
    private bool TryGoToClosestFoodToEat()
    {
        Transform seed = perception.GetClosestSeedInsidePheromoneForEating();

        if (seed == null)
        {
            return false;
        }

        FoodEffect food = seed.GetComponent<FoodEffect>();

        if (food == null)
        {
            return false;
        }
            

        // Do not target big seeds unless they are already cracked.
        if (food.needsCrack && !food.cracked)
            return false;

        currentTarget = seed;
        agent.SetDestination(currentTarget.position);
        currentState = AntState.GoingToFoodToEat;
        return true;
    }

    private Transform GetCloserTransform(Transform a, Transform b)
    {
        if (a == null) return b;
        if (b == null) return a;

        float distA = Vector2.Distance(transform.position, a.position);
        float distB = Vector2.Distance(transform.position, b.position);

        return distA <= distB ? a : b;
    }

    private void HandleGoingToFoodToEat()
    {
        if (currentTarget == null)
        {
            BeginIdle();
            return;
        }

        FoodEffect food = currentTarget.GetComponent<FoodEffect>();
        if (food == null)
        {
            currentTarget = null;
            BeginIdle();
            return;
        }

        agent.SetDestination(currentTarget.position);

        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist <= eatDistance)
        {
            EatCurrentTarget();
        }
    }

    private void EatCurrentTarget()
    {
        if (currentTarget == null)
        {
            BeginIdle();
            return;
        }

        FoodEffect food = currentTarget.GetComponent<FoodEffect>();

        if (food == null)
        {
            currentTarget = null;
            BeginIdle();
            return;
        }

        if (food.needsCrack && !food.cracked)
        {
            currentTarget = null;
            BeginIdle();
            return;
        }

        food.Consume(gameObject);

        currentTarget = null;
        BeginIdle();
    }
    //SOLDIER
    private bool TryGoToCrackableSeed()
    {
        if (perception == null)
            return false;

        if (!npcDefinition.cracksBigSeeds)
            return false;

        Transform seed = perception.GetClosestCrackableSeedInsidePheromone();

        if (seed == null)
            return false;

        currentTarget = seed;
        agent.SetDestination(currentTarget.position);
        currentState = AntState.GoingToCrackSeed;
        return true;
    }

    private void HandleGoingToCrackSeed()
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

        FoodEffect food = currentTarget.GetComponent<FoodEffect>();

        if (food == null || !food.needsCrack || food.cracked)
        {
            currentTarget = null;
            BeginIdle();
            return;
        }

        agent.SetDestination(currentTarget.position);

        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist <= pickupDistance)
        {
            food.cracked = true;

            Debug.Log($"{name} cracked {currentTarget.name}");

            currentTarget = null;
            BeginIdle();
        }
    }
}