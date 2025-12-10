using System.Collections.Generic;
using UnityEngine;

public class Current : MonoBehaviour
{
    [Header("Current Settings")]
    public Vector3 currentDirection = new Vector3(0f, 0f, 1f); // local direction of the current
    public float strength = 10f; // how fast the current moves the world

    private HashSet<FakePlayerMovement> affectedPlayers = new HashSet<FakePlayerMovement>();

    private void OnTriggerEnter(Collider other)
    {
        FakePlayerMovement player = other.GetComponent<FakePlayerMovement>();
        if (player == null)
            player = other.GetComponentInParent<FakePlayerMovement>();

        if (player != null && !affectedPlayers.Contains(player))
        {
            affectedPlayers.Add(player);
            Debug.Log($"Player entered current: {gameObject.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FakePlayerMovement player = other.GetComponent<FakePlayerMovement>();
        if (player == null)
            player = other.GetComponentInParent<FakePlayerMovement>();

        if (player != null && affectedPlayers.Contains(player))
        {
            affectedPlayers.Remove(player);
            Debug.Log($"Player exited current: {gameObject.name}");
        }
    }

    private void Update()
    {
        foreach (FakePlayerMovement player in affectedPlayers)
        {
            if (player == null) continue;

            // Add current movement to the player
            // We convert the local direction to world direction
            Vector3 worldMove = player.playerModel.TransformDirection(currentDirection.normalized) * strength;
            
            // Apply it like extra input for MoveWorld
            player.currentSpeed = Mathf.Max(player.currentSpeed, worldMove.z); // forward
            player.verticalInput = worldMove.y; // vertical
        }
    }
}
