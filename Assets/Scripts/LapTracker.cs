using UnityEngine;

public class LapTracker : MonoBehaviour
{
    [Header("Lap Data")]
    public int lapCount = 0;

    // internal flags
    private bool hitA = false;
    private bool hitB = false;

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

                // Reset for next lap loop
                hitA = false;
                hitB = false;
                break;
        }
    }
}
