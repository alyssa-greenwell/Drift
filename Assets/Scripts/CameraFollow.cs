using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
   public Vector3 offset = new Vector3(0, 5, -10);

    public float rotationSpeed = 5f;
    public float positionSmooth = 5f;
    

    void LateUpdate()
    {
        if (player == null) return;

        // Smooth position
        Vector3 targetPos = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, positionSmooth * Time.deltaTime);

        // Smooth rotation
        Quaternion targetRot = Quaternion.LookRotation(player.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }
}
