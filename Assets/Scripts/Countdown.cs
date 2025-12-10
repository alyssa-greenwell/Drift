using System.Collections;
using UnityEngine;
using TMPro;

public class Countdown : MonoBehaviour
{
    [Header("References")]
    public Transform turtle;              // Your player
    public Canvas countdownCanvas;        // World-space canvas
    public TextMeshProUGUI countdownText; // TextMeshPro text

    [Header("Settings")]
    public float distanceInFront = 8f;    // Distance from player
    public float heightOffset = 1.5f;     // Height above player
    public float timePerCount = 1f;
    public float canvasScale = 0.02f;     // Size of the canvas

    [Header("Audio (Optional)")]
    public AudioClip countSound;          // Sound for 3, 2, 1
    public AudioClip goSound;             // Sound for Go!
    private AudioSource audioSource;

    [Header("Animation Settings")]
    public bool enableScaleAnimation = true;
    public float scaleAmount = 1.5f;
    public float scaleDuration = 0.3f;

    private FakePlayerMovement playerMovement;
    private Vector3 originalScale;
    private bool countdownActive = true;

    private void Start()
    {
        // Get player movement reference
        if (turtle != null)
            playerMovement = turtle.GetComponent<FakePlayerMovement>();

        // Disable player movement at start
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (countSound != null || goSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0; // 2D sound
        }

        // Setup canvas for world space
        if (countdownCanvas != null)
        {
            // FORCE World Space settings
            countdownCanvas.renderMode = RenderMode.WorldSpace;
            countdownCanvas.worldCamera = null; // Don't need event camera for display
            
            // Set scale
            countdownCanvas.transform.localScale = new Vector3(canvasScale, canvasScale, canvasScale);
            
            // Make sure it's active
            countdownCanvas.gameObject.SetActive(true);
            
            Debug.Log($"Canvas setup: Active={countdownCanvas.gameObject.activeSelf}, RenderMode={countdownCanvas.renderMode}");
        }

        // Store original scale for animation
        if (countdownText != null)
        {
            originalScale = countdownText.transform.localScale;
            countdownText.gameObject.SetActive(true);
            Debug.Log($"Text setup: Active={countdownText.gameObject.activeSelf}, Text='{countdownText.text}'");
        }

        // Position canvas immediately before starting countdown
        UpdateCanvasPosition();

        StartCoroutine(DoCountdown());
    }

    private IEnumerator DoCountdown()
    {
        string[] counts = new string[] { "3", "2", "1", "GO!" };

        for (int i = 0; i < counts.Length; i++)
        {
            if (countdownText != null)
            {
                countdownText.text = counts[i];
                Debug.Log($"Countdown: {counts[i]}");

                // Scale animation
                if (enableScaleAnimation)
                    StartCoroutine(ScaleText());

                // Play sound
                if (audioSource != null)
                {
                    if (i < counts.Length - 1 && countSound != null)
                        audioSource.PlayOneShot(countSound);
                    else if (i == counts.Length - 1 && goSound != null)
                        audioSource.PlayOneShot(goSound);
                }
            }

            yield return new WaitForSeconds(timePerCount);
        }

        // Countdown finished
        countdownActive = false;
        Debug.Log("Countdown finished, hiding canvas");

        // Hide canvas
        if (countdownCanvas != null)
            countdownCanvas.gameObject.SetActive(false);

        // Enable movement
        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private IEnumerator ScaleText()
    {
        if (countdownText == null) yield break;

        Transform textTransform = countdownText.transform;
        Vector3 targetScale = originalScale * scaleAmount;

        // Scale up
        float elapsed = 0f;
        while (elapsed < scaleDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (scaleDuration / 2f);
            textTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Scale back down
        elapsed = 0f;
        while (elapsed < scaleDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (scaleDuration / 2f);
            textTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        textTransform.localScale = originalScale;
    }

    private void Update()
    {
        // Always update position during countdown
        if (countdownActive && countdownCanvas != null && turtle != null)
        {
            UpdateCanvasPosition();
        }
    }

    private void UpdateCanvasPosition()
    {
        if (countdownCanvas == null || turtle == null) return;

        // Position canvas in front of turtle
        Vector3 forward = turtle.forward;
        Vector3 targetPos = turtle.position + forward * distanceInFront + Vector3.up * heightOffset;
        countdownCanvas.transform.position = targetPos;

        // Make canvas face the turtle
        countdownCanvas.transform.LookAt(turtle.position + Vector3.up * heightOffset);
        countdownCanvas.transform.Rotate(0, 180, 0); // Flip so text reads correctly

        // Debug visualization
        Debug.DrawLine(turtle.position, targetPos, Color.green);
        Debug.DrawRay(targetPos, Vector3.up * 2f, Color.red);
        Debug.DrawRay(targetPos, countdownCanvas.transform.right * 2f, Color.blue);
    }

    // Optional: Public method to restart countdown
    public void RestartCountdown()
    {
        StopAllCoroutines();
        countdownActive = true;
        
        if (playerMovement != null)
            playerMovement.enabled = false;
        
        if (countdownCanvas != null)
            countdownCanvas.gameObject.SetActive(true);
        
        if (countdownText != null && originalScale != Vector3.zero)
            countdownText.transform.localScale = originalScale;
        
        StartCoroutine(DoCountdown());
    }

    private void OnDrawGizmos()
    {
        // Draw a sphere where the canvas should be
        if (turtle != null && countdownActive)
        {
            Vector3 forward = turtle.forward;
            Vector3 targetPos = turtle.position + forward * distanceInFront + Vector3.up * heightOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 1f);
        }
    }
}