using UnityEngine;

public class LapTracker : MonoBehaviour
{
    [Header("References")]
    public GameObject finaleTrigger;

    [Header("Lap Data")]
    public int lapCount = 0;

    // internal flags
    private bool hitA = false;
    private bool hitB = false;

    // Start is called before the first frame update
    private void Start()
    {
        // This ensures the finale trigger is HIDDEN when the game begins
        finaleTrigger?.SetActive(false);
    }

    // Called by checkpoint triggers
    public void HitCheckpoint(string id)
    {
        switch (id)
        {
            case "A":
                hitA = true;
                 Debug.Log("Hit Checkpoint A");
                break;

            case "B":
                if (hitA) hitB = true;
                 Debug.Log("Hit Checkpoint B");
                break;

            case "START":
                if (hitA && hitB)
                {
                    lapCount++;
                    Debug.Log("Lap Completed! Lap = " + lapCount);
                }

                if (lapCount >= 2)
                {
                    finaleTrigger.SetActive(true);
                    Debug.Log("Finale Trigger Activated!");
                }

                // Reset for next lap loop
                hitA = false;
                hitB = false;
                break;
        }
    }
}
