using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position;
            newPosition.z = transform.position.z; // Kameranın Z yüksekliğini koru
            transform.position = newPosition;
        }
    }
}
