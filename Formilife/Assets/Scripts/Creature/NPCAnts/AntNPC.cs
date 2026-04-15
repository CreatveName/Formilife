using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AntNPC : MonoBehaviour
{
    private enum AntState
    {
        Idle,
        Moving
    }

    [Header("Definition")]
    [SerializeField] private AntDefinition antDefinition;

    [Header("Movement")]
    [SerializeField] private float arriveDistance = 0.1f;
    [SerializeField] private float maxMoveTime = 3f;

    private Rigidbody2D rb;
    private Vector2 homePosition;
    private Vector2 targetPosition;
    private float idleTimer;
    private float moveTimer;
    private AntState currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        if (currentState == AntState.Idle)
        {
            HandleIdle();
        }
    }

    private void FixedUpdate()
    {
        if (currentState == AntState.Moving)
        {
            HandleMovement();
        }
    }

    private void HandleIdle()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            PickNewTarget();
            moveTimer = maxMoveTime;
            currentState = AntState.Moving;
        }
    }

    private void HandleMovement()
    {
        moveTimer -= Time.fixedDeltaTime;

        Vector2 currentPosition = rb.position;
        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            antDefinition.moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);

        if (Vector2.Distance(currentPosition, targetPosition) <= arriveDistance)
        {
            BeginIdle();
            return;
        }

        if (moveTimer <= 0f)
        {
            BeginIdle();
        }
    }

    private void BeginIdle()
    {
        idleTimer = Random.Range(antDefinition.minIdleTime, antDefinition.maxIdleTime);
        currentState = AntState.Idle;
    }

    private void PickNewTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * antDefinition.wanderRadius;
        targetPosition = homePosition + randomOffset;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = Application.isPlaying ? (Vector3)homePosition : transform.position;
        float radius = antDefinition != null ? antDefinition.wanderRadius : 3f;
        Gizmos.DrawWireSphere(center, radius);
    }
}