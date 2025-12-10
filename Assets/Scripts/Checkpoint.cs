using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public string checkpointID = "A"; // A, B, START

    private void OnTriggerEnter(Collider other)
    {

        if (!other.CompareTag("Player")) return;

        Debug.Log("Player hit checkpoint " + checkpointID);

        LapTracker tracker = other.GetComponent<LapTracker>();

        if (tracker != null)
        {
        tracker.HitCheckpoint(checkpointID);
            Debug.Log("tracker obtained");
        }
    }
}
