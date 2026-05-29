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
        GoingToCrackSeed,

        RecruitedFollow,
        RecruitedHelpCarry
    }

    [Header("Definition")]
    [SerializeField] private AntDefinition antDefinition;
    [SerializeField] private AntNPCDefinition npcDefinition;

    [Header("Pheromone Work")]
    [SerializeField] private float arriveDistance = 0.25f;
    [SerializeField] private float pickupDistance = 0.25f;
    [SerializeField] private float storageDropDistance = 0.8f;
    [SerializeField] private float seedInteractDistance = 0.45f;
    [SerializeField] private float stuckTimeout = 2.5f;
    [SerializeField] private float rotationSpeed = 540f;

    [Header("Debug")]
    [SerializeField] private AntState currentState;
    [Header("Survival")]
    [SerializeField] private float eatDistance = 0.35f;
    [Header("Queen Egg Laying")]
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private Transform eggSpawnPoint;
    [SerializeField] private float eggLayInterval = 8f;
    [SerializeField] private float eggSpawnRadius = 0.5f;
    [Header("Queen Throne Room")]
    [SerializeField] private Chamber throneRoom;
    [SerializeField] private float throneRoomWanderRadius = 2f;

    private float eggTimer;
    private float stuckTimer;
    private Vector3 lastPosition;

    private AntNeeds needs;

    private NavMeshAgent agent;
    private AntPerception perception;
    private NPCAntPickup pickup;
    private Transform recruitedLeader;
    private Transform recruitedCarryTarget;
    private Vector3 recruitedFollowOffset;
    private int recruitedSlotIndex;
    private float recruitedFollowSpacing = 1.1f;
    private bool isRecruited;
    private Collider2D[] antColliders;

    private Transform currentTarget;
    private float idleTimer;
    private bool IsBeingCarried()
    {
        return transform.parent != null;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<AntPerception>();
        pickup = GetComponent<NPCAntPickup>();
        needs = GetComponent<AntNeeds>();
        antColliders = GetComponentsInChildren<Collider2D>();

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
        eggTimer = eggLayInterval;
        lastPosition = transform.position;
        BeginIdle();
    }

    private void Update()
    {
        if (!CanUseAgent())
            return;

        if (npcDefinition.role == AntRole.Queen)
        {
            bool insideThrone =
                PheromoneManager.Instance != null &&
                PheromoneManager.Instance.IsInsideThroneRoom(transform.position);

            if (!insideThrone && !IsBeingCarried())
            {
                agent.ResetPath();
                currentTarget = null;
                currentState = AntState.Idle;
                return;
            }
        }
        
        FaceMovementDirection();
        
        if (isRecruited)
        {
            HandleRecruitedBehavior();
            return;
        }
    
        if (TryInterruptForHunger())
            return;
        
        HandleQueenEggLaying();

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
            if (needs != null && needs.IsHungry())
                return;

            if (throneRoom != null)
                WanderInsideThroneRoom();
            else
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
            if (npcDefinition.role == AntRole.Queen)
        {
            if (throneRoom != null)
            {
                float distanceFromThrone =
                    Vector3.Distance(transform.position, throneRoom.transform.position);

                if (distanceFromThrone > throneRoomWanderRadius + 1f)
                {
                    agent.SetDestination(throneRoom.transform.position);
                    return;
                }
            }

            if (HasArrived())
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

        if (PheromoneManager.Instance.IsInsideFoodStorage(currentTarget.position))
        {
            currentTarget = null;
            BeginIdle();
            return;
        }

        agent.SetDestination(currentTarget.position);

        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist <= seedInteractDistance || IsStuck())
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

        agent.SetDestination(currentTarget.position);

        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist <= storageDropDistance)
        {
            if (pickup != null && pickup.IsHoldingSomething)
            {
                // An NPC brought a seed to a chamber — completes the "Pave the Path" quest.
                GameObject delivered = pickup.HeldItem != null ? pickup.HeldItem.GameObject : null;
                pickup.Drop();

                if (delivered != null && delivered.GetComponent<FoodEffect>() != null &&
                    QuestButton.Instance != null)
                {
                    QuestButton.Instance.NotifySeedDelivered();
                }
            }

            currentTarget = null;
            BeginIdle();
            return;
        }

        if (IsStuck())
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

        stuckTimer = 0f;
        lastPosition = transform.position;
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

        if (dist <= eatDistance || IsStuck())
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

    private bool IsStuck()
    {
        if (Vector2.Distance(transform.position, lastPosition) < 0.02f)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
            lastPosition = transform.position;
        }

        return stuckTimer >= stuckTimeout;
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
    /////////QUEEEN
    private void HandleQueenEggLaying()
    {
        if (npcDefinition.role != AntRole.Queen)
            return;

        if (!npcDefinition.laysEggs)
            return;

        if (eggPrefab == null)
            return;

        // Optional: queen should not lay eggs while hungry
        if (needs != null && needs.IsHungry())
            return;

        bool insideThrone =
            PheromoneManager.Instance != null &&
            PheromoneManager.Instance.IsInsideThroneRoom(transform.position);

        bool beingCarried = transform.parent != null;

        // ONLY lay eggs if inside throne room AND not carried
        if (!insideThrone || beingCarried)
            return;

        eggTimer -= Time.deltaTime;

        if (eggTimer > 0f)
            return;

        Vector3 center = eggSpawnPoint != null ? eggSpawnPoint.position : transform.position;
        Vector2 randomOffset = Random.insideUnitCircle * eggSpawnRadius;
        Vector3 spawnPos = center + new Vector3(randomOffset.x, randomOffset.y, 0f);

        Instantiate(eggPrefab, spawnPos, Quaternion.identity);

        Debug.Log($"{name} laid an egg.");

        eggTimer = eggLayInterval;
    }

    public void AssignThroneRoom(Chamber chamber)
    {
        if (npcDefinition.role != AntRole.Queen)
            return;

        throneRoom = chamber;

        if (throneRoom != null)
            Debug.Log($"{name} assigned throne room: {throneRoom.name}");

        BeginIdle();
    }

    private void WanderInsideThroneRoom()
    {
        if (throneRoom == null)
        {
            WanderToRandomPheromonePoint();
            return;
        }

        Vector3 center = throneRoom.transform.position;

        for (int i = 0; i < 15; i++)
        {
            Vector2 offset = Random.insideUnitCircle * throneRoomWanderRadius;
            Vector3 randomPoint = center + new Vector3(offset.x, offset.y, 0f);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                currentState = AntState.WanderingInPheromone;
                return;
            }
        }

        agent.SetDestination(center);
        currentState = AntState.WanderingInPheromone;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (npcDefinition == null || npcDefinition.role != AntRole.Queen)
            return;

        Chamber chamber = other.GetComponent<Chamber>();

        if (chamber != null && chamber.current == ChamberRole.ThroneRoom)
        {
            AssignThroneRoom(chamber);
        }
    }

    ///////RECRUITMENT
    public bool CanBeRecruited()
    {
        if (npcDefinition == null)
            return false;

        return npcDefinition.role == AntRole.Worker || npcDefinition.role == AntRole.Soldier;
    }

    public AntRole GetRole()
    {
        if (npcDefinition == null)
            return AntRole.Worker;

        return npcDefinition.role;
    }

    public void RecruitToPlayer(Transform player, int slotIndex, float followSpacing)
    {
        if (player == null)
            return;

        if (!CanBeRecruited())
            return;

        isRecruited = true;
        recruitedLeader = player;
        recruitedCarryTarget = null;
        recruitedSlotIndex = slotIndex;
        recruitedFollowSpacing = followSpacing;

        recruitedFollowOffset = GetFollowOffset(slotIndex, recruitedFollowSpacing);

        if (pickup != null && pickup.IsHoldingSomething)
        {
            pickup.Drop();
        }

        currentTarget = null;

        SetRecruitCollision(false);

        currentState = AntState.RecruitedFollow;

        if (CanUseAgent())
            agent.ResetPath();

        Debug.Log($"{name} recruited by player.");
    }

    // Backwards-compatible overload in case another script still calls the old method.
    public void RecruitToPlayer(Transform player)
    {
        RecruitToPlayer(player, 0, 1.1f);
    }

    public void HelpCarry(Transform carryTarget, int slotIndex)
    {
        if (!isRecruited)
            return;

        recruitedCarryTarget = carryTarget;
        recruitedSlotIndex = slotIndex;
        currentState = AntState.RecruitedHelpCarry;
    }

    // Backwards-compatible overload in case another script still calls the old method.
    public void HelpCarry(Transform carryTarget)
    {
        HelpCarry(carryTarget, recruitedSlotIndex);
    }

    public void StopHelpingCarry()
    {
        if (!isRecruited)
            return;

        recruitedCarryTarget = null;
        currentState = AntState.RecruitedFollow;
    }

    public void DismissRecruit()
    {
        isRecruited = false;
        recruitedLeader = null;
        recruitedCarryTarget = null;

        if (CanUseAgent())
            agent.ResetPath();
        
        SetRecruitCollision(true);

        BeginIdle();

        Debug.Log($"{name} dismissed.");
    }
    private void FaceMovementDirection()
    {
        Vector2 velocity = agent.velocity;

        if (velocity.sqrMagnitude < 0.01f)
            return;

        float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + 90f;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleRecruitedBehavior()
    {
        if (recruitedLeader == null)
        {
            DismissRecruit();
            return;
        }

        if (!CanUseAgent())
            return;

        if (currentState == AntState.RecruitedHelpCarry && recruitedCarryTarget != null)
        {
            agent.SetDestination(GetCarrySlotPosition(recruitedSlotIndex));

            Vector2 dir = recruitedCarryTarget.position - transform.position;

            if (dir.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;

                Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            return;
        }

        Vector3 followPos = recruitedLeader.position + recruitedFollowOffset;
        agent.SetDestination(followPos);
        currentState = AntState.RecruitedFollow;
    }

    private Vector3 GetFollowOffset(int slotIndex, float spacing)
    {
        // Puts recruited ants behind and around the player instead of stacking directly on them.
        Vector2[] offsets =
        {
            new Vector2(0f, -1f),
            new Vector2(-0.8f, -0.8f),
            new Vector2(0.8f, -0.8f),
            new Vector2(-1.2f, -0.2f),
            new Vector2(1.2f, -0.2f),
            new Vector2(0f, -1.6f)
        };

        Vector2 offset = offsets[slotIndex % offsets.Length].normalized * spacing;
        return new Vector3(offset.x, offset.y, 0f);
    }

    private Vector3 GetCarrySlotPosition(int slotIndex)
    {
        if (recruitedCarryTarget == null)
            return transform.position;

        // Slots around the carried item. This makes helpers look like they are actually assisting.
        Vector2[] slots =
        {
            Vector2.left,
            Vector2.right,
            Vector2.down,
            new Vector2(-0.7f, -0.7f),
            new Vector2(0.7f, -0.7f),
            Vector2.up
        };

        Vector2 offset = slots[slotIndex % slots.Length].normalized * 0.7f;
        return recruitedCarryTarget.position + new Vector3(offset.x, offset.y, 0f);
    }
    private void SetRecruitCollision(bool enabledState)
    {
        foreach (Collider2D col in antColliders)
        {
            if (col.isTrigger)
                continue;

            col.enabled = enabledState;
        }
    }
}
