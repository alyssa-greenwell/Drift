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
    public float minHeightOffset = -200f;
    public float maxHeightOffset = 100f;
    public float currentYOffset = 0f;
    private float startingY;

    float forwardInput;
    float turnInput;
    float verticalInput;

    float currentSpeed = 0f;

    void Start()
{
    if (worldRoot != null)
        startingY = worldRoot.transform.position.y;
}

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

        // Forward/back (Y axis)
        forwardInput += stick.y;

        // Turn (X axis, reversed)
        turnInput += -stick.x;

        // Triggers for vertical rotation
        float lt = Gamepad.current.leftTrigger.ReadValue();   // down
        float rt = Gamepad.current.rightTrigger.ReadValue();  // up
        verticalInput += (rt - lt);
    }

    // Clamp inputs
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

    // Forward/backward movement
    Vector3 move = playerModel.forward * currentSpeed;

    // Vertical movement
    if (Mathf.Abs(verticalInput) > 0.01f)
    {
        // Update vertical offset
        currentYOffset += verticalInput * verticalSpeed * Time.deltaTime;
        currentYOffset = Mathf.Clamp(currentYOffset, minHeightOffset, maxHeightOffset);

        // Apply vertical movement to world
        Vector3 worldPos = worldRoot.transform.position;
        worldPos.y = startingY + currentYOffset;
        worldRoot.transform.position = worldPos;
    }

    // Horizontal movement
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
