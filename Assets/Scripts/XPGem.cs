using UnityEngine;

public class XPGem : MonoBehaviour
{
    public int xpValue = 1;

    public float attractionSpeed = 8f;
    private Transform playerTransform;
    private PlayerStats playerStats;
    private const string PlayerTag = "Player";
    private const string XPGemTag = "XPGem";
    
    private bool isMagnetized = false; // Is globally magnetized?

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
        }
    }

    public void Magnetize()
    {
        isMagnetized = true;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        float currentAttractionRange = (playerStats != null) ? playerStats.magnetRange : 5f;

        // If magnetized or within range, move to player
        if (isMagnetized || distance <= currentAttractionRange)
        {
            // Accelerate if magnetized
            float speed = isMagnetized ? attractionSpeed * 2.5f : attractionSpeed;
            
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                speed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(PlayerTag))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddXP(xpValue);
            }

            // G�NCELLEND�: Topland�ktan sonra havuza geri g�nder
            if (ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.ReturnToPool(XPGemTag, gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
