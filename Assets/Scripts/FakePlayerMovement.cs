using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FakePlayerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject worldRoot;
    public Transform playerTransform; // usually your player object

    [Header("Movement Settings")]
    public float acceleration = 15f;
    public float maxSpeed = 30f;
    public float drag = 2f;
    public float verticalSpeed = 10f;

    [Header("Steering Settings")]
    public float turnSpeed = 80f;
    public float pitchSpeed = 40f;
    public float maxPitchAngle = 45f;

    [Header("Auto-level")]
    public float autoLevelSpeed = 2f;

    private Vector3 currentVelocity = Vector3.zero;
    private Quaternion playerRotation;

    void Start()
    {
        playerRotation = playerTransform.rotation;
    }

    void FixedUpdate()
    {
        // --- INPUT ---
        Gamepad gamepad = Gamepad.current;
        Keyboard kb = Keyboard.current;

        float moveInput = 0f;
        float strafeInput = 0f;
        float steerInput = 0f;
        float pitchInput = 0f;
        float verticalInput = 0f;

        if (gamepad != null)
        {
            moveInput = gamepad.leftStick.ReadValue().y;
            strafeInput = gamepad.leftStick.ReadValue().x;
            steerInput = gamepad.rightStick.ReadValue().x;
            pitchInput = -gamepad.rightStick.ReadValue().y;
            if (gamepad.rightShoulder.isPressed) verticalInput += 1f;
            if (gamepad.leftShoulder.isPressed) verticalInput -= 1f;
        }

        if (kb != null)
        {
            if (kb.wKey.isPressed) moveInput += 1f;
            if (kb.sKey.isPressed) moveInput -= 1f;
            if (kb.aKey.isPressed) strafeInput -= 1f;
            if (kb.dKey.isPressed) strafeInput += 1f;
            if (kb.qKey.isPressed) verticalInput -= 1f;
            if (kb.eKey.isPressed) verticalInput += 1f;
            if (kb.leftArrowKey.isPressed) steerInput -= 1f;
            if (kb.rightArrowKey.isPressed) steerInput += 1f;
            if (kb.upArrowKey.isPressed) pitchInput += 1f;
            if (kb.downArrowKey.isPressed) pitchInput -= 1f;
        }

        // --- WORLDROOT MOVEMENT ---
        Vector3 forward = playerTransform.forward;
        Vector3 right = playerTransform.right;
        Vector3 up = playerTransform.up;

        // Move WorldRoot opposite to input
        Vector3 targetVelocity = (forward * moveInput + right * strafeInput) * acceleration;
        targetVelocity += up * verticalInput * verticalSpeed;

        // Smooth movement
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * 5f);
        if (currentVelocity.magnitude > maxSpeed)
            currentVelocity = currentVelocity.normalized * maxSpeed;

        worldRoot.transform.position -= currentVelocity * Time.fixedDeltaTime;

        // --- ROTATION ---
        if (Mathf.Abs(steerInput) > 0.1f)
        {
            float turnAmount = steerInput * turnSpeed * Time.fixedDeltaTime;
            worldRoot.transform.Rotate(Vector3.up, -turnAmount, Space.World);
        }

        // Pitch auto-level
        Vector3 currentEuler = playerTransform.localEulerAngles;
        float currentPitch = currentEuler.x;
        if (currentPitch > 180f) currentPitch -= 360f;

        if (Mathf.Abs(pitchInput) > 0.1f)
        {
            float pitchAmount = pitchInput * pitchSpeed * Time.fixedDeltaTime;
            playerTransform.Rotate(playerTransform.right, pitchAmount, Space.World);
        }
        else if (Mathf.Abs(currentPitch) > 1f)
        {
            float levelAmount = Mathf.MoveTowards(currentPitch, 0f, autoLevelSpeed * Time.fixedDeltaTime);
            currentEuler.x = levelAmount;
            playerTransform.localEulerAngles = currentEuler;
        }
    }
}
