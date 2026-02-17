using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerWeaponController : MonoBehaviour
{
    public WeaponData weaponData;

    [Header("Weapon Placement")]
    [Tooltip("Attach an empty GameObject at the character's hand position here.")]
    public Transform firePoint;

    [Header("Charged Shot Settings")]
    public float chargedShotTime = 1.0f;
    public GameObject beamPrefab;
    
    private float chargeTimer = 0f;
    private bool isCharging = false;

    private float fireTimer;
    private bool isFiring = false;

    private Animator animator;

    // Input Actions
    private InputSystem_Actions inputActions;
    private InputAction chargedShotAction;

    void OnEnable()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        
        chargedShotAction = inputActions.Player.ChargedShot;
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();

        if (firePoint == null)
        {
            firePoint = transform;
            Debug.LogWarning("FirePoint not assigned! Projectiles will spawn from character center.");
        }
    }

    void Update()
    {
        // Game paused check
        if (Time.timeScale == 0) return;

        HandleChargedShot();
        HandleAutoFire();
    }

    void HandleChargedShot()
    {
        // Input Action for charged shot (supports both mouse and gamepad)
        bool isPressed = chargedShotAction.IsPressed();
        
        if (isPressed)
        {
            isCharging = true;
            chargeTimer += Time.deltaTime;
        }
        else if (!isPressed && isCharging)
        {
            if (chargeTimer >= chargedShotTime)
            {
                FireChargedBeam();
            }
            isCharging = false;
            chargeTimer = 0f;
        }
    }

    void FireChargedBeam()
    {
        if (beamPrefab == null)
        {
            Debug.LogWarning("Beam Prefab not assigned!");
            return;
        }

        // Find nearest enemy to aim at
        Transform nearestEnemy = FindNearestEnemy();
        Vector3 targetPos;

        if (nearestEnemy != null)
        {
            targetPos = nearestEnemy.position;
        }
        else
        {
            // No enemy found, fire in last movement direction or down
            targetPos = firePoint.position + Vector3.down;
        }

        Vector2 dir = (targetPos - firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Instantiate(beamPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
        
        Debug.Log("Charged Beam FIRED!");
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, weaponData.Range);
        
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            EnemyAI enemy = col.GetComponent<EnemyAI>();
            BossAI boss = col.GetComponent<BossAI>();
            
            if (enemy != null || boss != null)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = col.transform;
                }
            }
        }

        return nearest;
    }

    void HandleAutoFire()
    {
        if (isFiring) return; 

        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            StartCoroutine(FireRoutine());
        }
    }

    IEnumerator FireRoutine()
    {
        isFiring = true;
        int shotsFired = 0;

        while (shotsFired < weaponData.projectileCount)
        {
            FindAndFireSingleShot();
            shotsFired++;

            if (shotsFired < weaponData.projectileCount)
            {
                yield return new WaitForSeconds(0.5f); 
            }
        }

        fireTimer = weaponData.Cooldown;
        isFiring = false;
    }

    void FindAndFireSingleShot()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, weaponData.Range);

        List<Transform> validTargets = new List<Transform>();
        HashSet<GameObject> addedEnemies = new HashSet<GameObject>();

        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject) continue;

            EnemyAI enemy = col.GetComponent<EnemyAI>();
            BossAI boss = col.GetComponent<BossAI>();
            
            if (enemy != null)
            {
                if (!addedEnemies.Contains(enemy.gameObject))
                {
                    validTargets.Add(enemy.transform);
                    addedEnemies.Add(enemy.gameObject);
                }
            }
            else if (boss != null)
            {
                if (!addedEnemies.Contains(boss.gameObject))
                {
                    validTargets.Add(boss.transform);
                    addedEnemies.Add(boss.gameObject);
                }
            }
        }

        // Sort by distance (nearest first)
        validTargets.Sort((a, b) => 
        {
            float distA = Vector3.Distance(transform.position, a.position);
            float distB = Vector3.Distance(transform.position, b.position);
            return distA.CompareTo(distB);
        });

        if (validTargets.Count > 0)
        {
            FireProjectile(validTargets[0].position);
        }
    }

    void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    void FireProjectile(Vector3 targetPosition)
    {
        TriggerAttackAnimation();

        GameObject projectileGO;
        
        // Spawn from firePoint position
        if (ObjectPooler.Instance != null)
        {
            projectileGO = ObjectPooler.Instance.SpawnFromPool("Projectile", firePoint.position, Quaternion.identity);
        }
        else
        {
            projectileGO = Instantiate(weaponData.ProjectilePrefab, firePoint.position, Quaternion.identity);
        }

        if (projectileGO != null)
        {
            Projectile projectile = projectileGO.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    weaponData.Damage, 
                    weaponData.ProjectileSpeed, 
                    targetPosition, 
                    weaponData.canBounce, 
                    weaponData.canExplode,
                    weaponData.bounceCount,
                    weaponData.explosionRadius,
                    weaponData.critChance,
                    weaponData.critDamageMultiplier
                );
            }
        }
    }
}