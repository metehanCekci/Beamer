using UnityEngine;

public class OrbitalProjectile : MonoBehaviour
{
    public float damage = 5f;

    void Start()
    {
        // Çarpışma için Collider gerekli
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            // Eğer yoksa CircleCollider2D ekle
            CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true; // Trigger olmalı
            circle.radius = 0.5f; // Varsayılan yarıçap (Sprite yoksa diye)
        }

        // Trigger olaylarının tetiklenmesi için en az bir tarafta Rigidbody olmalı
        // Düşmanda varsa sorun yok ama garanti olsun diye buraya da ekleyelim (Kinematic)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // Fiziksel kuvvetlerden etkilenmesin
            rb.gravityScale = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Enemy Check
        EnemyAI enemy = other.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, false); // Orbitals don't crit for now
            enemy.ApplyKnockback(transform.position, 3f); // Small knockback
            return;
        }

        // Boss Check
        BossAI boss = other.GetComponent<BossAI>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            // No knockback for boss
        }
    }
}