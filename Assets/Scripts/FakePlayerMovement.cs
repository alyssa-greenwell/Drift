using UnityEngine;
using UnityEngine.InputSystem;

public class FakePlayerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject worldRoot;
    public Transform playerModel;

    [Header("Movement Settings")]
    public float acceleration = 5f;
    public float deceleration = 4f;
    public float maxSpeed = 15f;
    public float verticalSpeed = 5f;
    public float turnSpeed = 80f;

    [Header("Collision Settings")]
    public float collisionCheckDistance = 1f;

    float forwardInput;
    float turnInput;
    float verticalInput;

    float currentSpeed = 0f;

    void Update()
    {
        ReadInput();
        ApplyAcceleration();
        RotateWorld();
        MoveWorld();
    }

    void ReadInput()
    {
        forwardInput = 0f;
        turnInput = 0f;
        verticalInput = 0f;

        // -------------------------
        // KEYBOARD INPUT
        // -------------------------
        if (Keyboard.current != null)
        {
            // Forward/back
            if (Keyboard.current.wKey.isPressed) forwardInput += 1f;
            if (Keyboard.current.sKey.isPressed) forwardInput -= 1f;

            // REVERSED TURNING (A = rotate world right, D = rotate world left)
            if (Keyboard.current.aKey.isPressed) turnInput += 1f;
            if (Keyboard.current.dKey.isPressed) turnInput -= 1f;

            // Vertical (Q down, E up)
            if (Keyboard.current.eKey.isPressed) verticalInput += 1f;
            if (Keyboard.current.qKey.isPressed) verticalInput -= 1f;
        }

        // -------------------------
        // GAMEPAD INPUT
        // -------------------------
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();

            // Forward-back (Y axis on left stick)
            forwardInput += stick.y;

            // Turn (X axis on stick, reversed like keyboard)
            turnInput += -stick.x;  // stick.x right should turn left, so we invert

            // Triggers for up/down
            float lt = Gamepad.current.leftTrigger.ReadValue();   // down
            float rt = Gamepad.current.rightTrigger.ReadValue();  // up

            verticalInput += (rt - lt);  // RT increases, LT decreases
        }

        // Clamp to avoid weird combos
        forwardInput = Mathf.Clamp(forwardInput, -1f, 1f);
        turnInput = Mathf.Clamp(turnInput, -1f, 1f);
        verticalInput = Mathf.Clamp(verticalInput, -1f, 1f);
    }

    void ApplyAcceleration()
    {
        if (Mathf.Abs(forwardInput) > 0.01f)
        {
            currentSpeed += forwardInput * acceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                deceleration * Time.deltaTime
            );
        }

        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
    }

    void RotateWorld()
    {
        if (worldRoot == null || Mathf.Abs(turnInput) < 0.01f) return;

        worldRoot.transform.RotateAround(
            playerModel.position,
            Vector3.up,
            turnInput * turnSpeed * Time.deltaTime
        );
    }

    void MoveWorld()
    {
        if (worldRoot == null) return;

        Vector3 move = playerModel.forward * currentSpeed;
        move += Vector3.up * verticalInput * verticalSpeed;

        if (!IsBlocked(move))
        {
            worldRoot.transform.position -= move * Time.deltaTime;
        }
    }

    bool IsBlocked(Vector3 move)
    {
        RaycastHit hit;
        Vector3 origin = playerModel.position;
        Vector3 dir = move.normalized;

        if (Physics.Raycast(origin, dir, out hit, collisionCheckDistance))
        {
            if (!hit.collider.isTrigger)
                return true;
        }

        return false;
    }
}
