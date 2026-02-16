using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossAI : MonoBehaviour
{
    public enum BossState { Idle, Chase, Shooting, Dashing }

    [Header("Stats")]
    public float maxHealth = 5000f;
    public float currentHealth;
    public float moveSpeed = 2f;
    public float dashSpeed = 10f;

    [Header("Combat")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float timeBetweenAttacks = 2f;
    public float attackCooldown = 0f;

    [Header("UI")]
    public Slider bossHealthBar;
    // public GameObject winPanel; // Boss ölünce açılacak panel (İptal edildi, yerine upgrade ekranı gelecek)

    private Transform player;
    private BossState currentState;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (bossHealthBar != null)
        {
            bossHealthBar.maxValue = maxHealth;
            bossHealthBar.value = currentHealth;
            bossHealthBar.gameObject.SetActive(true);
        }

        currentState = BossState.Chase;
    }

    private void Update()
    {
        if (player == null) return;

        attackCooldown -= Time.deltaTime;

        switch (currentState)
        {
            case BossState.Chase:
                HandleChase();
                break;
            case BossState.Shooting:
                // Shooting is handled by coroutine usually, but we can do simple logic
                break;
            case BossState.Dashing:
                // Dashing logic
                break;
        }

        // State switching logic
        if (currentState == BossState.Chase && attackCooldown <= 0)
        {
            // Randomly choose an attack
            int rand = Random.Range(0, 2);
            if (rand == 0) StartCoroutine(ShootAttack());
            else StartCoroutine(DashAttack());
        }
    }

    private void HandleChase()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Flip sprite
        if (direction.x > 0) spriteRenderer.flipX = false;
        else spriteRenderer.flipX = true;
    }

    private IEnumerator ShootAttack()
    {
        currentState = BossState.Shooting;
        rb.linearVelocity = Vector2.zero; // Stop moving to shoot

        // Flash color to indicate attack
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.color = originalColor;

        // Shoot 3 waves of bullets
        for (int i = 0; i < 3; i++)
        {
            if (player == null) break;
            
            // Calculate direction to player
            Vector2 dir = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Spawn projectile
            Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle));
            
            // Also spawn 2 angled projectiles (Shotgun style)
            Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle + 15));
            Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle - 15));

            yield return new WaitForSeconds(0.3f);
        }

        attackCooldown = timeBetweenAttacks;
        currentState = BossState.Chase;
    }

    private IEnumerator DashAttack()
    {
        currentState = BossState.Dashing;
        rb.linearVelocity = Vector2.zero;

        // Warning indicator
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.color = originalColor;

        if (player != null)
        {
            Vector2 dashDir = (player.position - transform.position).normalized;
            rb.linearVelocity = dashDir * dashSpeed;
        }

        yield return new WaitForSeconds(1f); // Dash duration

        rb.linearVelocity = Vector2.zero;
        attackCooldown = timeBetweenAttacks;
        currentState = BossState.Chase;
    }

    public void TakeDamage(float amount)
    {
        // Overload Reward: Player deals 2x damage
        if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.Overload))
        {
            amount *= 2f;
        }

        // Executioner Reward: Double damage if HP < 20%
        if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.Executioner))
        {
            if (currentHealth < maxHealth * 0.2f)
            {
                amount *= 2f;
            }
        }

        // Vampire Reward: Heal Player
        if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.Vampire))
        {
            if (player != null)
            {
                PlayerStats ps = player.GetComponent<PlayerStats>();
                if (ps != null)
                {
                    ps.Heal(amount * 0.1f); // 10% Lifesteal
                }
            }
        }

        currentHealth -= amount;
        if (bossHealthBar != null)
        {
            bossHealthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    [Header("Reward UI")]
    public BossRewardUI rewardUI; // Assign in Inspector

    private void Die()
    {
        // Stop all logic
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;

        Debug.Log("Boss Defeated!");

        if (rewardUI != null)
        {
            rewardUI.ShowRewards();
        }
        else
        {
            Debug.LogWarning("Reward UI not assigned! Destroying Boss...");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player mermileriyle çarpışma kontrolü
        // Normalde Projectile scripti çarpınca kendini yok eder ve hasar verir.
        // Ancak Projectile scripti şu an EnemyAI arıyor olabilir.
        // Projectile scriptini BossAI'yi de kapsayacak şekilde güncellememiz gerekebilir.
        
        // Şimdilik Projectile scriptine dokunmadan, buraya basit bir kontrol ekleyelim
        // Eğer merminin tag'i "PlayerProjectile" ise (veya Projectile scripti varsa)
        
        Projectile proj = collision.GetComponent<Projectile>();
        if (proj != null)
        {
            TakeDamage(proj.damage);
            // Mermiyi yok et (Projectile scripti zaten kendini yok ediyor olabilir ama garanti olsun)
            Destroy(collision.gameObject);
        }
        
        // Orbital Weapon kontrolü
        OrbitalProjectile orbital = collision.GetComponent<OrbitalProjectile>();
        if (orbital != null)
        {
             TakeDamage(orbital.damage);
             // Orbital mermiler yok olmaz, içinden geçer
        }
    }
}
