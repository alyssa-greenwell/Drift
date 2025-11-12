using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main Camera")]
    public GameObject mainCam;

    [Header("Movement Settings")]
    public float acceleration = 15f;
    public float maxSpeed = 30f;
    public float drag = 2f;
    public float verticalSpeed = 10f;

    [Header("Steering Settings")]
    public float turnSpeed = 80f;
    public float pitchSpeed = 40f;
    public float maxPitchAngle = 45f;

    [Header("Underwater Physics")]
    public float waterDrag = 1.5f;
    public float waterAngularDrag = 3f;
    public float autoLevelSpeed = 2f;

    [Header("Reset Settings")]
    public float resetSpeed = 0.5f;

    private Rigidbody rb;
    private Quaternion resetAngle;
    private Vector3 resetPosition;
    private float currentSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearDamping = waterDrag;
        rb.angularDamping = waterAngularDrag;

        resetAngle = transform.rotation;
        resetPosition = transform.position;
    }

    void FixedUpdate()
    {
        // --- INPUT ---
        Gamepad gamepad = Gamepad.current;

        // Reset to home position (A / Cross button or R key)
        bool resetPressed = false;
        if (gamepad != null) resetPressed = gamepad.buttonSouth.isPressed;
        if (Keyboard.current != null) resetPressed |= Keyboard.current.rKey.isPressed;

        if (resetPressed)
        {
            transform.position = Vector3.Slerp(transform.position, resetPosition, resetSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, resetAngle, resetSpeed * Time.deltaTime);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            currentSpeed = 0f;
            return;
        }

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

        // Testing keyboard controls
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput -= 1f;
            if (Keyboard.current.aKey.isPressed) strafeInput -= 1f;
            if (Keyboard.current.dKey.isPressed) strafeInput += 1f;
            if (Keyboard.current.qKey.isPressed) verticalInput -= 1f;
            if (Keyboard.current.eKey.isPressed) verticalInput += 1f;
            if (Keyboard.current.leftArrowKey.isPressed) steerInput -= 1f;
            if (Keyboard.current.rightArrowKey.isPressed) steerInput += 1f;
            if (Keyboard.current.upArrowKey.isPressed) pitchInput += 1f;
            if (Keyboard.current.downArrowKey.isPressed) pitchInput -= 1f;
        }

        // --- MOVEMENT ---
        if (Mathf.Abs(moveInput) > 0.1f)
            currentSpeed += acceleration * moveInput * Time.fixedDeltaTime;
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, drag * Time.fixedDeltaTime);

        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);

        Vector3 forwardMove = mainCam.transform.forward * currentSpeed;
        Vector3 strafeMove = mainCam.transform.right * strafeInput * (maxSpeed * 0.5f);
        Vector3 verticalMove = mainCam.transform.up * verticalInput * verticalSpeed;
        rb.linearVelocity = forwardMove + strafeMove + verticalMove;

        // --- ROTATION ---
        if (Mathf.Abs(steerInput) > 0.1f)
        {
            float turnAmount = steerInput * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }

        // Pitch
        Vector3 currentEuler = transform.localEulerAngles;
        float currentPitch = currentEuler.x;
        if (currentPitch > 180f) currentPitch -= 360f;

        if (Mathf.Abs(pitchInput) > 0.1f)
        {
            if (Mathf.Abs(currentPitch) < maxPitchAngle || 
                (currentPitch > 0 && pitchInput < 0) || 
                (currentPitch < 0 && pitchInput > 0))
            {
                float pitchAmount = pitchInput * pitchSpeed * Time.fixedDeltaTime;
                transform.Rotate(mainCam.transform.right, pitchAmount, Space.World);
            }
        }
        else if (Mathf.Abs(currentPitch) > 1f)
        {
            float levelAmount = Mathf.MoveTowards(currentPitch, 0f, autoLevelSpeed * Time.fixedDeltaTime);
            currentEuler.x = levelAmount;
            transform.localEulerAngles = currentEuler;
        }
    }
}
