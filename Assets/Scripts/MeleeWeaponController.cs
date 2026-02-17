using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Melee weapon controller for close-range characters like Vity.
/// Creates short-range damaging waves around the player.
/// </summary>
public class MeleeWeaponController : MonoBehaviour
{
    public WeaponData weaponData;

    [Header("Melee Settings")]
    public float attackRadius = 2f;
    public int attackSegments = 3; // Number of attack directions
    public float attackArcAngle = 120f; // Arc angle for each attack
    public Vector2 attackCenterOffset = new Vector2(0f, 0.5f); // Offset from character pivot (up for chest level)
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private float slashDuration = 0.2f;

    private float attackTimer;
    private Vector2 lastMoveDirection = Vector2.down; // Track last movement direction

    private Animator animator;

    // No input actions needed for auto-attack melee weapon

    void Start()
    {
        animator = GetComponent<Animator>();

        if (weaponData == null)
        {
            Debug.LogError("WeaponData not assigned to MeleeWeaponController!");
        }
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        // Update movement direction for visual effects
        UpdateMovementDirection();
        
        HandleAutoAttack();
    }

    void UpdateMovementDirection()
    {
        // Get movement input from PlayerMovement component
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.linearVelocity.magnitude > 0.1f)
            {
                lastMoveDirection = rb.linearVelocity.normalized;
            }
        }
    }

    void HandleAutoAttack()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            PerformArcAttack();
            attackTimer = weaponData.Cooldown;
        }
    }

    void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    void PerformArcAttack()
    {
        // Get facing angle from movement direction
        float facingAngle = Mathf.Atan2(lastMoveDirection.y, lastMoveDirection.x) * Mathf.Rad2Deg;

        TriggerAttackAnimation();

        // Attack in a wide arc in front of player
        DamageInArc(facingAngle, attackArcAngle);
        SpawnSlashEffect(facingAngle);
    }

    void DamageInArc(float centerAngle, float arcAngle)
    {
        Vector2 attackCenter = (Vector2)transform.position + attackCenterOffset;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, attackRadius);
        
        Debug.Log($"[MeleeWeapon] Arc attack at angle {centerAngle:F0}°, found {hits.Length} colliders in radius");
        
        HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();
        int enemiesInArc = 0;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // Check if enemy is within the attack arc
            Vector2 dirToEnemy = ((Vector2)hit.transform.position - attackCenter).normalized;
            float angleToEnemy = Mathf.Atan2(dirToEnemy.y, dirToEnemy.x) * Mathf.Rad2Deg;
            
            // Normalize angles
            float normalizedCenter = NormalizeAngle(centerAngle);
            float normalizedEnemyAngle = NormalizeAngle(angleToEnemy);
            
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(normalizedCenter, normalizedEnemyAngle));

            if (angleDifference <= arcAngle / 2f)
            {
                enemiesInArc++;
                // Enemy is within arc
                EnemyAI enemy = hit.GetComponent<EnemyAI>();
                BossAI boss = hit.GetComponent<BossAI>();

                if (enemy != null && !damagedEnemies.Contains(enemy.gameObject))
                {
                    // Calculate critical hit
                    bool isCritical = Random.value * 100f < weaponData.critChance;
                    float damage = weaponData.Damage;
                    if (isCritical) damage *= weaponData.critDamageMultiplier;

                    enemy.TakeDamage(damage, isCritical);
                    damagedEnemies.Add(enemy.gameObject);
                    Debug.Log($"[MeleeWeapon] Hit enemy! Damage: {damage:F1}, Critical: {isCritical}");
                }
                else if (boss != null && !damagedEnemies.Contains(boss.gameObject))
                {
                    bool isCritical = Random.value * 100f < weaponData.critChance;
                    float damage = weaponData.Damage;
                    if (isCritical) damage *= weaponData.critDamageMultiplier;

                    boss.TakeDamage(damage);
                    damagedEnemies.Add(boss.gameObject);
                    Debug.Log($"[MeleeWeapon] Hit boss! Damage: {damage:F1}, Critical: {isCritical}");
                }
            }
        }
        
        Debug.Log($"[MeleeWeapon] Damaged {damagedEnemies.Count} enemies (total in arc: {enemiesInArc})");
    }

    float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }

    void SpawnSlashEffect(float angle)
    {
        if (slashEffectPrefab == null) return;

        Vector3 attackCenter = (Vector2)transform.position + attackCenterOffset;
        Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.right;
        Vector3 spawnPos = attackCenter + direction * (attackRadius * 0.5f);

        GameObject slash = Instantiate(slashEffectPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
        Destroy(slash, slashDuration);
    }

    void OnDrawGizmosSelected()
    {
        // Draw attack radius
        Vector3 attackCenter = (Vector2)transform.position + attackCenterOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCenter, attackRadius);

        // Draw attack segments
        if (attackSegments > 0)
        {
            float anglePerSegment = 360f / attackSegments;
            Gizmos.color = Color.yellow;
            
            for (int i = 0; i < attackSegments; i++)
            {
                float angle = i * anglePerSegment * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                Gizmos.DrawLine(attackCenter, attackCenter + direction * attackRadius);
            }
        }
    }
}
