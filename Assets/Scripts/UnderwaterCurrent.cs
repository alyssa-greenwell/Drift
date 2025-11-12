/* Underwater Current Volume - Sonic Riders style wind currents
   Creates boost zones that push players in a specific direction
   Similar to Sonic Riders' turbulence/slipstream mechanic
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterCurrent : MonoBehaviour
{
    [Header("Current Properties")]
    [Tooltip("Direction and strength of the current")]
    public Vector3 currentForce = new Vector3(0, 0, 50f);
    
    [Tooltip("How the current affects the player: Push (adds force) or Override (sets velocity)")]
    public CurrentMode mode = CurrentMode.Push;
    
    [Tooltip("How quickly the current accelerates the player (Push mode only)")]
    public float accelerationRate = 10f;
    
    [Tooltip("Maximum speed the current can push the player to")]
    public float maxCurrentSpeed = 60f;

    [Header("Visual Effects")]
    [Tooltip("Particle system for current visualization")]
    public ParticleSystem currentParticles;
    
    [Tooltip("Color of the current particles")]
    public Color currentColor = new Color(0.3f, 0.7f, 1f, 0.5f);
    
    [Tooltip("Show debug arrows in editor")]
    public bool showDebugArrows = true;

    [Header("Sound")]
    [Tooltip("Sound that plays when entering the current")]
    public AudioClip enterCurrentSound;
    
    [Tooltip("Looping sound while in current")]
    public AudioClip currentLoopSound;
    
    private AudioSource audioSource;

    [Header("Boost Settings")]
    [Tooltip("Speed multiplier applied when in current")]
    public float speedBoostMultiplier = 1.5f;
    
    [Tooltip("Does this current grant temporary invincibility?")]
    public bool grantsInvincibility = false;
    
    [Tooltip("Duration of invincibility in seconds")]
    public float invincibilityDuration = 2f;

    private HashSet<Rigidbody> affectedRigidbodies = new HashSet<Rigidbody>();
    private Dictionary<Rigidbody, Coroutine> boostCoroutines = new Dictionary<Rigidbody, Coroutine>();

    public enum CurrentMode
    {
        Push,      // Adds force gradually (more natural)
        Override   // Instantly sets velocity (more arcade-like)
    }

    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (enterCurrentSound != null || currentLoopSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.maxDistance = 50f;
        }

        // Setup particles if assigned
        if (currentParticles != null)
        {
            var main = currentParticles.main;
            main.startColor = currentColor;
            
            // Make particles flow in current direction
            var shape = currentParticles.shape;
            shape.rotation = Quaternion.LookRotation(currentForce.normalized).eulerAngles;
        }

        // Make sure we have a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void FixedUpdate()
    {
        // Apply force to all rigidbodies in the current
        foreach (Rigidbody rb in affectedRigidbodies)
        {
            if (rb == null) continue;

            if (mode == CurrentMode.Push)
            {
                // Gradually accelerate the player
                Vector3 desiredVelocity = currentForce.normalized * maxCurrentSpeed;
                Vector3 velocityChange = desiredVelocity - rb.linearVelocity;
                velocityChange = Vector3.ClampMagnitude(velocityChange, accelerationRate * Time.fixedDeltaTime);
                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            else if (mode == CurrentMode.Override)
            {
                // Instantly set velocity (more arcade-like)
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, currentForce, accelerationRate * Time.fixedDeltaTime);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        // Add to affected list (works with any rigidbody)
        if (!affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Add(rb);

            // Play enter sound
            if (audioSource != null && enterCurrentSound != null)
            {
                audioSource.PlayOneShot(enterCurrentSound);
            }

            // Start looping sound
            if (audioSource != null && currentLoopSound != null && !audioSource.isPlaying)
            {
                audioSource.clip = currentLoopSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            // Visual feedback
            if (currentParticles != null && !currentParticles.isPlaying)
            {
                currentParticles.Play();
            }

            Debug.Log($"Object entered current: {gameObject.name} - {other.gameObject.name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        // Remove from affected list (works with any rigidbody)
        if (affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Remove(rb);

            // Stop looping sound if no more rigidbodies
            if (affectedRigidbodies.Count == 0 && audioSource != null)
            {
                audioSource.Stop();
            }

            // Stop boost coroutine
            if (boostCoroutines.ContainsKey(rb))
            {
                StopCoroutine(boostCoroutines[rb]);
                boostCoroutines.Remove(rb);
            }

            Debug.Log($"Object exited current: {gameObject.name} - {other.gameObject.name}");
        }
    }

    IEnumerator ApplySpeedBoost(Rigidbody rb)
    {
        // Optional: Add custom speed boost logic here
        // This is separate from the force-based physics

        // Grant invincibility if enabled
        if (grantsInvincibility)
        {
            // You can add invincibility logic here
            yield return new WaitForSeconds(invincibilityDuration);
        }

        // Keep active while in current
        while (affectedRigidbodies.Contains(rb))
        {
            yield return null;
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugArrows) return;

        // Draw the current direction
        Gizmos.color = currentColor;
        Vector3 center = GetComponent<Collider>() != null ? 
            GetComponent<Collider>().bounds.center : transform.position;
        
        // Draw arrow showing current direction
        Vector3 direction = currentForce.normalized * 5f;
        Gizmos.DrawRay(center, direction);
        
        // Draw arrowhead
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * Vector3.forward;
        Gizmos.DrawRay(center + direction, right * 1f);
        Gizmos.DrawRay(center + direction, left * 1f);

        // Draw the trigger volume
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.3f);
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw more detailed info when selected
        Gizmos.color = Color.cyan;
        Vector3 center = GetComponent<Collider>() != null ? 
            GetComponent<Collider>().bounds.center : transform.position;
        
        // Draw speed indicator
        float speedMagnitude = currentForce.magnitude;
        Gizmos.DrawWireSphere(center + currentForce.normalized * 3f, speedMagnitude * 0.1f);
    }
}