using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    public GameObject damagePopupPrefab; // Inspector'dan atanacak (TextMeshPro bileşeni olan bir obje)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Transform playerTransform;

    public void Create(Vector3 position, int damageAmount, bool isCritical = false)
    {
        if (damagePopupPrefab == null) return;

        // Find Player (If not found yet)
        if (playerTransform == null)
        {
            // Search by Tag first
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            // If not found, search by name (Backup)
            if (player == null)
            {
                player = GameObject.Find("Player");
            }

            if (player != null) playerTransform = player.transform;
        }

        Vector3 spawnPosition;

        // If Player found, use their position (Override 'position' parameter)
        if (playerTransform != null)
        {
            // DamagePopup will handle offset internally
            spawnPosition = playerTransform.position;
        }
        else
        {
            // If no player, use old method
            spawnPosition = position + new Vector3(0, 1f, 0);
        }
        
        spawnPosition.z = -2f; // To be in front of camera

        GameObject popupTransform = Instantiate(damagePopupPrefab, spawnPosition, Quaternion.identity);
        DamagePopup popup = popupTransform.GetComponent<DamagePopup>();
        if (popup != null)
        {
            popup.Setup(damageAmount, playerTransform, isCritical);
        }
    }
}
