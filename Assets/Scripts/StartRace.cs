using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StartRaceManagerTMP : MonoBehaviour
{
    public Button startButton;                     // Assign your Start button here
    public TMP_Text countdownText;                 // Assign your TextMeshPro UI here
    public FakePlayerMovement playerMovement;      // Assign your turtle movement script

    [Header("Countdown Settings")]
    public float displayTime = 1f;                 // Total time each number shows
    public float fadeTime = 0.5f;                  // Fade in/out time

    void Start()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;       // Disable movement at start

        if (startButton != null)
            startButton.onClick.AddListener(StartCountdown);
    }

    void StartCountdown()
    {
        if (startButton != null)
            startButton.gameObject.SetActive(false);

        StartCoroutine(CountdownCoroutine());
    }

    IEnumerator CountdownCoroutine()
    {
        string[] countdown = new string[] { "3", "2", "1", "Go!" };
        foreach (string s in countdown)
        {
            countdownText.text = s;
            yield return StartCoroutine(FadeText(countdownText, displayTime, fadeTime));
        }

        countdownText.text = "";
        if (playerMovement != null)
            playerMovement.enabled = true;        // Enable movement
    }

    IEnumerator FadeText(TMP_Text text, float totalTime, float fadeTime)
    {
        float half = fadeTime;
        Color original = text.color;
        Color c = original;

        // Fade in
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(0, 1, t / half);
            text.color = c;
            yield return null;
        }

        c.a = 1;
        text.color = c;

        // Wait remaining time
        yield return new WaitForSeconds(totalTime - 2 * half);

        // Fade out
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1, 0, t / half);
            text.color = c;
            yield return null;
        }

        c.a = 0;
        text.color = c;
    }
}
