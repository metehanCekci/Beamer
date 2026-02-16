using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;
    public float speed;
    private Vector3 targetPosition;
    private const string ProjectileTag = "Projectile";

    // Yeni özellikler
    private bool canBounce;
    private bool canExplode;
    private int maxBounceCount;
    private int currentBounceCount;
    private float explosionRadius;
    private float critChance;
    private float critDamageMultiplier;

    public void Initialize(float dmg, float spd, Vector3 targetPos, bool bounce = false, bool explode = false, int maxBounces = 0, float radius = 2f, float cChance = 0f, float cDmgMult = 1.5f)
    {
        damage = dmg;
        speed = spd;
        targetPosition = targetPos;
        canBounce = bounce;
        canExplode = explode;
        maxBounceCount = maxBounces;
        currentBounceCount = 0;
        explosionRadius = radius;
        critChance = cChance;
        critDamageMultiplier = cDmgMult;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Hedefe çok yaklaştıysa mermiyi havuza geri gönder
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // GÜNCELLENDİ: Destroy yerine ReturnToPool kullan
            if (ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.ReturnToPool(ProjectileTag, gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Tag kontrolü yerine Component kontrolü yapıyoruz
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        BossAI boss = other.GetComponent<BossAI>();
        
        if (enemy != null)
        {
            HandleEnemyHit(enemy, other.transform.position);
        }
        else if (boss != null)
        {
            HandleBossHit(boss, other.transform.position);
        }
    }

    private void HandleBossHit(BossAI boss, Vector3 hitPosition)
    {
        // Boss için kritik vuruş hesabı
        float finalDamage = damage;
        bool isCritical = Random.Range(0f, 100f) < critChance;
        if (isCritical) finalDamage *= critDamageMultiplier;

        if (HitStopManager.Instance != null) HitStopManager.Instance.Stop(isCritical ? 0.1f : 0.05f);

        boss.TakeDamage(finalDamage);

        // Boss'a knockback uygulanmaz (genelde)
        
        // Mermiyi yok et
        if (ObjectPooler.Instance != null)
            ObjectPooler.Instance.ReturnToPool(ProjectileTag, gameObject);
        else
            Destroy(gameObject);
    }

    private void HandleEnemyHit(EnemyAI enemy, Vector3 enemyPosition)
    {
            // Kritik Vuruş Hesabı
            float finalDamage = damage;
            bool isCritical = Random.Range(0f, 100f) < critChance;
            if (isCritical)
            {
                finalDamage *= critDamageMultiplier;
                
                // Kritik vuruşta Screen Shake
                if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.15f, 0.2f);
            }

            // Hit Stop (Hit Feel)
            if (HitStopManager.Instance != null)
            {
                // Stop longer if critical
                HitStopManager.Instance.Stop(isCritical ? 0.1f : 0.05f);
            }

            // Deal damage to enemy
            enemy.TakeDamage(finalDamage, isCritical);
            
            // Apply Knockback
            // Hardcoded force for now, can be moved to WeaponData later
            enemy.ApplyKnockback(transform.position, 5f);

            bool shouldDestroy = true;

            if (canExplode)
            {
                Explode(enemyPosition);
            }
            else if (canBounce && currentBounceCount < maxBounceCount)
            {
                if (FindNextTarget(enemyPosition))
                {
                    shouldDestroy = false;
                    currentBounceCount++;
                }
            }

            if (shouldDestroy)
            {
                // Return projectile to pool
                if (ObjectPooler.Instance != null)
                {
                    ObjectPooler.Instance.ReturnToPool(ProjectileTag, gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
    }

    private void Explode(Vector3 center)
    {
        // Screen Shake for explosion effect
        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.2f, 0.4f);

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, explosionRadius);

        foreach (var hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                if (hit.gameObject.activeInHierarchy)
                {
                    enemy.TakeDamage(damage, false); // Explosion damage (no crit for now)
                    enemy.ApplyKnockback(center, 8f); // Stronger knockback for explosion
                }
            }
        }
    }

    private bool FindNextTarget(Vector3 currentPos)
    {
        // Tüm aktif düşmanları bul (Tag bağımsız)
        // Unity 2023+ için FindObjectsByType kullanımı (Daha performanslı)
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        
        Transform nearest = null;
        float minDst = float.MaxValue;

        foreach (var enemyAI in enemies)
        {
            // Kendisine (veya az önce vurduğu düşmana) tekrar sekmemesi için mesafe kontrolü
            if (enemyAI.gameObject.activeInHierarchy)
            {
                float dst = Vector3.Distance(currentPos, enemyAI.transform.position);
                
                // Çok yakınsa (aynı düşmansa) atla
                if (dst < 0.1f) continue;

                if (dst < minDst)
                {
                    minDst = dst;
                    nearest = enemyAI.transform;
                }
            }
        }

        if (nearest != null)
        {
            targetPosition = nearest.position;
            return true;
        }
        return false;
    }
}