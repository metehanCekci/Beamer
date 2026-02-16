using UnityEngine;

/// <summary>
/// Coin that can be picked up by the player.
/// Flies toward player when in magnet range.
/// </summary>
public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private float magnetSpeed = 10f;
    [SerializeField] private float pickupDistance = 0.5f;
    [SerializeField] private float lifetime = 30f; // Despawn after 30 seconds

    private Transform player;
    private PlayerStats playerStats;
    private bool isBeingMagneted = false;
    private float spawnTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        spawnTime = Time.time;
    }

    void Update()
    {
        // Despawn after lifetime
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (player == null || playerStats == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if within magnet range
        if (distanceToPlayer <= playerStats.magnetRange)
        {
            isBeingMagneted = true;
        }

        // Fly toward player if magneted
        if (isBeingMagneted)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * magnetSpeed * Time.deltaTime;

            // Pickup if close enough
            if (distanceToPlayer <= pickupDistance)
            {
                PickupCoin();
            }
        }
    }

    void PickupCoin()
    {
        // Add coins to PersistentGameManager
        if (PersistentGameManager.Instance != null)
        {
            PersistentGameManager.Instance.totalCoins += coinValue;
            int totalCoins = PersistentGameManager.Instance.totalCoins;
            PersistentGameManager.Instance.SaveData();
            
            Debug.Log($"💰 Coin Picked Up! +{coinValue} | Total Coins: {totalCoins}");
        }

        // Visual/Audio feedback can be added here
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PickupCoin();
        }
    }
}
