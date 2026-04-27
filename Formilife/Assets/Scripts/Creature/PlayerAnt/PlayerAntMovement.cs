using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAntMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float turnSpeed = 180f;

    [Header("Carrying")]
    [SerializeField] private float weightFactor = 0.5f;
    [SerializeField] private bool weightSlowsTurn = true;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float walkAnimReferenceSpeed = 3f;

    private Rigidbody2D rb;
    private PlayerPickup carrier; // for new pickup system

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        carrier = GetComponent<PlayerPickup>();

        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float turnInput = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) turnInput += 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) turnInput -= 1f;

        float moveInput = 0f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveInput += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveInput -= 1f;

        bool isRunning = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed
            || keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        
        //new weight system bcuz pickup updated
        float heldWeight = carrier != null ? carrier.CurrentCarryWeight : 0f;

        if (carrier != null && carrier.HeldItem != null)
        {
            heldWeight = carrier.HeldItem.Weight;
        }

        float weightMultiplier = 1f / (1f + heldWeight * weightFactor);

        currentSpeed *= weightMultiplier;

        float currentTurnSpeed =
            weightSlowsTurn ? turnSpeed * weightMultiplier : turnSpeed;
        
        //end of new pickup system for the slow turn/movement bcuz of weight 

        float newRotation = rb.rotation + turnInput * currentTurnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(newRotation);

        Vector2 forward = (Vector2)transform.up;
        rb.MovePosition(rb.position + forward * (moveInput * currentSpeed * Time.fixedDeltaTime));

        if (animator != null)
        {
            float locomotion = Mathf.Abs(moveInput) * currentSpeed + Mathf.Abs(turnInput) * 0.5f;
            animator.speed = locomotion / Mathf.Max(0.0001f, walkAnimReferenceSpeed);
        }
    }
}
