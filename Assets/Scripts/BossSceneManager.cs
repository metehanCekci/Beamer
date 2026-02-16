using UnityEngine;

public class BossSceneManager : MonoBehaviour
{
    public float bossCameraSize = 5f; // Zoomed in size
    public Transform playerSpawnPoint;

    void Start()
    {
        // 1. Find Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Position Player at spawn point
            if (playerSpawnPoint != null)
                player.transform.position = playerSpawnPoint.position;

            // Manual control removed - Auto-fire everywhere (Vampire Survivors style)
            // PlayerWeaponController already auto-fires by default

            // 2. Adjust Camera
            Camera.main.orthographicSize = bossCameraSize;
            
            // Ensure Camera follows player (if CameraController exists)
            CameraController camController = Camera.main.GetComponent<CameraController>();
            if (camController != null)
            {
                camController.target = player.transform;
            }
        }
    }
}
