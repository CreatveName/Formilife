using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AntNPC : MonoBehaviour
{
    private enum AntState
    {
        Idle,
        Wandering,
        GoingToTargetZone,
        SearchingForPickup,
        GoingToPickup,
        ReturningToStartZone
    }

    [Header("Definition")]
    [SerializeField] private AntDefinition antDefinition;

    [Header("Route Work")]
    [SerializeField] private float arriveDistance = 0.25f;
    [SerializeField] private float pickupDistance = 0.25f;
    [SerializeField] private float searchTimeAtTarget = 2f;

    [Header("Debug")]
    [SerializeField] private AntState currentState;

    private NavMeshAgent agent;
    private AntPerception perception;
    private NPCAntPickup pickup;

    private Vector2 homePosition;
    private Transform currentTarget;
    private float idleTimer;
    private float searchTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<AntPerception>();
        pickup = GetComponent<NPCAntPickup>();

        // Important for 2D NavMesh
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

        homePosition = transform.position;
        agent.speed = antDefinition.moveSpeed;

        BeginIdle();
    }

    private void Update()
    {
        PheromoneRoute route = PheromoneRouteManager.Instance != null
            ? PheromoneRouteManager.Instance.ActiveRoute
            : null;

        switch (currentState)
        {
            case AntState.Idle:
                HandleIdle(route);
                break;

            case AntState.Wandering:
                if (HasArrived())
                    BeginIdle();
                break;

            case AntState.GoingToTargetZone:
                if (route == null || !route.IsValid)
                {
                    BeginIdle();
                    return;
                }

                if (HasArrived())
                {
                    BeginSearchingForPickup();
                }
                break;

            case AntState.SearchingForPickup:
                HandleSearchingForPickup(route);
                break;

            case AntState.GoingToPickup:
                HandleGoingToPickup(route);
                break;

            case AntState.ReturningToStartZone:
                HandleReturningToStart(route);
                break;
        }
    }

    private void HandleIdle(PheromoneRoute route)
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer > 0f)
            return;

        if (route != null && route.IsValid)
        {
            GoToTargetZone(route);
        }
        else
        {
            PickRandomWanderTarget();
        }
    }

    private void GoToTargetZone(PheromoneRoute route)
    {
        currentTarget = null;
        agent.SetDestination(route.TargetPosition);
        currentState = AntState.GoingToTargetZone;
    }

    private void BeginSearchingForPickup()
    {
        searchTimer = searchTimeAtTarget;
        currentState = AntState.SearchingForPickup;
    }

    private void HandleSearchingForPickup(PheromoneRoute route)
    {
        if (route == null || !route.IsValid)
        {
            BeginIdle();
            return;
        }

        searchTimer -= Time.deltaTime;

        Transform foundPickup = perception.GetClosestPickupable();

        if (foundPickup != null)
        {
            currentTarget = foundPickup;
            agent.SetDestination(currentTarget.position);
            currentState = AntState.GoingToPickup;
            return;
        }

        if (searchTimer <= 0f)
        {
            GoToTargetZone(route);
        }
    }

    private void HandleGoingToPickup(PheromoneRoute route)
    {
        if (route == null || !route.IsValid)
        {
            BeginIdle();
            return;
        }

        if (currentTarget == null)
        {
            BeginSearchingForPickup();
            return;
        }

        agent.SetDestination(currentTarget.position);

        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist <= pickupDistance)
        {
            TryPickUpTarget(route);
        }
    }

    private void TryPickUpTarget(PheromoneRoute route)
    {
        IPickupable item = currentTarget.GetComponent<IPickupable>();

        if (item != null)
        {
            bool success = pickup.TryPickUp(item);

            if (success)
            {
                currentTarget = null;
                agent.SetDestination(route.StartPosition);
                currentState = AntState.ReturningToStartZone;
                return;
            }
        }

        currentTarget = null;
        BeginSearchingForPickup();
    }

    private void HandleReturningToStart(PheromoneRoute route)
    {
        if (route == null || !route.IsValid)
        {
            BeginIdle();
            return;
        }

        agent.SetDestination(route.StartPosition);

        if (HasArrived())
        {
            pickup.Drop();
            GoToTargetZone(route);
        }
    }

    private void BeginIdle()
    {
        idleTimer = Random.Range(antDefinition.minIdleTime, antDefinition.maxIdleTime);
        currentTarget = null;
        currentState = AntState.Idle;
    }

    private void PickRandomWanderTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * antDefinition.wanderRadius;
        Vector2 destination = homePosition + randomOffset;

        agent.SetDestination(destination);
        currentState = AntState.Wandering;
    }

    private bool HasArrived()
    {
        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= arriveDistance;
    }
}