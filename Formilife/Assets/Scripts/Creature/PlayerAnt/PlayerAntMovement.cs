using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAntMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float turnSpeed = 180f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
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

        float newRotation = rb.rotation + turnInput * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(newRotation);

        Vector2 forward = (Vector2)transform.up;
        rb.MovePosition(rb.position + forward * (moveInput * currentSpeed * Time.fixedDeltaTime));
    }
}
