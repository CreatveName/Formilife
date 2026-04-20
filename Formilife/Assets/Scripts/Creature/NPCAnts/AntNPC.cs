using System.Runtime.Serialization;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AntNPC : MonoBehaviour
{
    private enum AntState
    {
        Idle,
        Moving,
        SeekingFood,
        //SeekingWater,
        Resting
    }

    [Header("Definition")]
    [SerializeField] private AntDefinition antDefinition;
    [SerializeField] private AntNeeds needs;

    [Header("Movement")]
    [SerializeField] private float arriveDistance = 0.1f;
    [SerializeField] private float maxMoveTime = 3f;
    [SerializeField] private float eatDistance = 0.2f;

    private Rigidbody2D rb;
    private Vector2 homePosition;
    private Vector2 targetPosition;
    private float idleTimer;
    private float moveTimer;
    private AntPerception perception;
    private NPCAntPickup pickup;
    private Transform currentTarget;
    [SerializeField]private AntState currentState;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        perception = GetComponent<AntPerception>();
        pickup = GetComponent<NPCAntPickup>();
    }

    private void Start()
    {
        if (antDefinition == null)
        {
            Debug.LogError($"{name} is missing an AntDefinition.", this);
            enabled = false;
            return;
        }

        homePosition = rb.position;
        BeginIdle();
    }

    private void Update()
    {
        if (needs != null && needs.IsHungry())
        {
            if (currentState != AntState.SeekingFood)
            {
                currentState = AntState.SeekingFood;
            }
        }
        switch (currentState)
        {
            case AntState.Idle:
                HandleIdle();
                break;

            case AntState.Moving:
                //UpdateFoodTargeting();
                break;

            case AntState.SeekingFood:
                UpdateFoodTargeting();
                break;

            //case AntState.SeekingWater:
                //HandleWaterTargeting(); // placeholder
            //    break;

            case AntState.Resting:
                HandleIdle();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (currentState == AntState.Moving)
        {
            HandleMovement();
        }
    }

    private void DecideNextAction()
    {
        if (needs != null)
        {
            if (needs.IsHungry())
            {
                currentState = AntState.SeekingFood;
                return;
            }

            if (needs.IsThirsty())
            {
                //currentState = AntState.SeekingWater;
                return;
            }

            if (needs.IsTired())
            {
                currentState = AntState.Resting;
                BeginIdle();
                return;
            }
        }

        currentState = AntState.Idle;
        BeginIdle();
    }

    private void HandleIdle()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            DecideNextAction();
        }
    }

    private void HandleMovement()
    {
        moveTimer -= Time.fixedDeltaTime;

        Vector2 destination;

        // If we have food, follow it dynamically
        if (currentTarget != null)
        {
            destination = currentTarget.position;
        }
        else
        {
            destination = targetPosition;
        }

        Vector2 currentPosition = rb.position;

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            destination,
            antDefinition.moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);

        float dist = Vector2.Distance(currentPosition, destination);

        if (dist <= eatDistance)
        {
            TryConsumeFood();
            return;
        }

        if (dist <= arriveDistance || moveTimer <= 0f)
        {
            currentTarget = null;
            BeginIdle();
        }
    }

    private void TryConsumeFood()
    {
        if (currentTarget == null)
        {
            BeginIdle();
            return;
        }

        // Try pickup system first (keeps your architecture intact)
        IPickupable item = currentTarget.GetComponent<IPickupable>();

        if (item != null)
        {
            bool success = pickup.TryPickUp(item);

            if (success)
            {
                FoodEffect effect = currentTarget.GetComponent<FoodEffect>();

                if (effect != null)
                {
                    effect.Consume(gameObject);
                }
            }
        }

        currentTarget = null;
        BeginIdle();
    }

    private void BeginIdle()
    {
        idleTimer = Random.Range(antDefinition.minIdleTime, antDefinition.maxIdleTime);

        currentState = AntState.Idle;

        // small chance to re-wander
        if (Random.value < 0.5f)
        {
            PickRandomTarget();
        }
    }

    private void PickRandomTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * antDefinition.wanderRadius;
        targetPosition = homePosition + randomOffset;

        moveTimer = maxMoveTime;
        currentState = AntState.Moving;
    }

    private void UpdateFoodTargeting()
    {
        Transform foundFood = perception.GetClosestFood();

        if (foundFood != null)
        {
            currentTarget = foundFood;
            targetPosition = foundFood.position;

            moveTimer = maxMoveTime;

            currentState = AntState.Moving;
        }
        else
        {
            // No food found → fall back to wandering
            currentTarget = null;
            PickRandomTarget();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = Application.isPlaying ? (Vector3)homePosition : transform.position;
        float radius = antDefinition != null ? antDefinition.wanderRadius : 3f;
        Gizmos.DrawWireSphere(center, radius);
    }
}