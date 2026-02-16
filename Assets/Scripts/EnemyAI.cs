using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3f;
    public float maxHealth = 20f;
    public float collisionDamage = 10f; // Damage dealt when colliding with Player
    public int xpDropAmount = 1; // Amount of XP dropped
    
    [Header("Coin Drops")]
    public int minCoins = 0;
    public int maxCoins = 3;
    
    [Header("Effects")]
    public GameObject deathEffectPrefab; // Assign in Inspector

    private float currentHealth;
    private Transform target;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // Animasyon için eklendi
    private Color originalColor;
    
    private float knockbackTimer; // Timer for knockback duration

    void OnEnable()
    {
        currentHealth = maxHealth;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Animator referansı alındı

        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    void Update()
    {
        // Her karede yön kontrolü yap
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (target == null) return;

        // Düşman oyuncunun sağındaysa sola, solundaysa sağa bakmalı
        // localScale kullanarak tüm objeyi (varsa silahı vs.) çeviriyoruz
        if (target.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (target.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // Tek animasyon olduğu için Animator'da herhangi bir parametre tetiklemeye gerek yok.
        // Animator'daki "Default State" olan animasyon otomatik olarak loop şeklinde oynayacaktır.
    }

    public void TakeDamage(float amount, bool isCritical = false)
    {
        if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.Overload))
        {
            amount *= 2f;
        }

        currentHealth -= amount;

        if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.Vampire))
        {
            if (target != null)
            {
                PlayerStats ps = target.GetComponent<PlayerStats>();
                if (ps != null)
                {
                    ps.Heal(amount * 0.1f);
                }
            }
        }
        
        if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.Executioner))
        {
            if (currentHealth < maxHealth * 0.2f)
            {
                currentHealth = 0;
                isCritical = true;
            }
        }
        
        StartCoroutine(FlashRoutine());

        if (DamagePopupManager.Instance != null)
        {
            if (target != null)
            {
                DamagePopupManager.Instance.Create(target.position, (int)amount, isCritical);
            }
            else
            {
                DamagePopupManager.Instance.Create(transform.position, (int)amount, isCritical);
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyKnockback(Vector3 sourcePosition, float force)
    {
        if (rb == null) return;

        Vector2 direction = (transform.position - sourcePosition).normalized;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        knockbackTimer = 0.2f;
    }

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
    }

    private void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Drop coins (create at runtime, no prefab needed)
        int coinsToDrop = Random.Range(minCoins, maxCoins + 1);
        for (int i = 0; i < coinsToDrop; i++)
        {
            Vector3 randomOffset = Random.insideUnitCircle * 0.5f;
            CreateCoin(transform.position + randomOffset);
        }

        // Drop XP
        if (EnemySpawner.Instance != null)
        {
            int finalXpAmount = xpDropAmount;
            
            if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.Bandit))
            {
                finalXpAmount *= 2;
            }

            for (int i = 0; i < finalXpAmount; i++)
            {
                Vector3 randomOffset = Random.insideUnitCircle * 0.5f;
                EnemySpawner.Instance.DropXP(transform.position + randomOffset);
            }
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null) stats.AddKill();
        }

        if (BossRewardManager.Instance != null)
        {
            BossRewardManager.Instance.OnEnemyKilled();
        }

        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool("Enemy", gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(collisionDamage);
                ApplyKnockback(other.transform.position, 5f);
            }
        }
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    void CreateCoin(Vector3 position)
    {
        // Create coin GameObject at runtime
        GameObject coin = new GameObject("Coin");
        coin.transform.position = position;

        // Add sprite renderer with yellow circle
        SpriteRenderer sr = coin.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(1f, 0.85f, 0f); // Gold color
        sr.sortingOrder = 5;

        // Add circle collider
        CircleCollider2D col = coin.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        // Add coin pickup script
        coin.AddComponent<CoinPickup>();
    }

    Sprite CreateCircleSprite()
    {
        // Create a simple circle sprite
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        Vector2 center = new Vector2(16, 16);
        float radius = 14f;
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * 32 + x] = distance <= radius ? Color.white : Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(collisionDamage);
                ApplyKnockback(collision.transform.position, 5f);
            }
        }
    }
}