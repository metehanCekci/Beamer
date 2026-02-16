using UnityEngine;
using System.Collections.Generic;

public class OrbitalWeapon : MonoBehaviour
{
    [Header("Settings")]
    public GameObject orbitalPrefab; // Rotating object prefab (Assign in Inspector)
    public float rotationSpeed = 100f;
    public float distance = 2.5f;
    public int orbitalCount = 0; // Initially 0
    public float damage = 10f;

    private List<GameObject> activeOrbitals = new List<GameObject>();
    
    // Center point for orbitals (Usually Player)
    private Transform centerPoint;
    private Transform orbitalHolder; // Intermediate object for rotation

    void Start()
    {
        centerPoint = transform;

        // Create orbital holder object
        if (orbitalHolder == null)
        {
            GameObject holderObj = new GameObject("OrbitalHolder");
            orbitalHolder = holderObj.transform;
            orbitalHolder.SetParent(transform);
            orbitalHolder.localPosition = Vector3.zero;
            orbitalHolder.localRotation = Quaternion.identity;
        }
        
        UpdateOrbitals();
    }

    private float shatteredCogsTimer = 0f;
    private float shatteredCogsInterval = 3f;

    void Update()
    {
        // Rotate the orbital holder
        if (orbitalHolder != null)
        {
            orbitalHolder.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }

        HandleShatteredCogs();
    }

    void HandleShatteredCogs()
    {
        if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.ShatteredCogs))
        {
            shatteredCogsTimer -= Time.deltaTime;
            if (shatteredCogsTimer <= 0f)
            {
                shatteredCogsTimer = shatteredCogsInterval;
                // Shatter orbitals outwards and damage enemies
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, distance * 2f);
                foreach (var hit in hits)
                {
                    EnemyAI enemy = hit.GetComponent<EnemyAI>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage * 0.5f);
                    }
                    
                    BossAI boss = hit.GetComponent<BossAI>();
                    if (boss != null)
                    {
                        boss.TakeDamage(damage * 0.5f);
                    }
                }
            }
        }
    }



    public void AddOrbital()
    {
        orbitalCount++;
        UpdateOrbitals();
    }

    public void IncreaseSpeed(float amount)
    {
        rotationSpeed += amount;
    }

    public void IncreaseDamage(float amount)
    {
        damage += amount;
        foreach(var orb in activeOrbitals)
        {
             var dmgScript = orb.GetComponent<OrbitalProjectile>();
             if(dmgScript) dmgScript.damage = damage;
        }
    }

    public void DecreaseDistance(float amount)
    {
        distance -= amount;
        if (distance < 1f) distance = 1f; // Minimum mesafe sınırı
        UpdateOrbitals();
    }

    public void SetStats(int count, float dmg)
    {
        orbitalCount = count;
        damage = dmg;
        UpdateOrbitals();
    }

    private void UpdateOrbitals()
    {
        // Clear existing orbitals
        foreach (var obj in activeOrbitals)
        {
            if (obj != null) Destroy(obj);
        }
        activeOrbitals.Clear();

        if (orbitalCount <= 0 || orbitalPrefab == null) return;

        float angleStep = 360f / orbitalCount;

        for (int i = 0; i < orbitalCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * distance;
            
            GameObject newOrb = Instantiate(orbitalPrefab, orbitalHolder.position + offset, Quaternion.identity, orbitalHolder);
            newOrb.transform.localPosition = offset;

            // Setup orbital projectile component
            OrbitalProjectile p = newOrb.GetComponent<OrbitalProjectile>();
            if (p == null) p = newOrb.AddComponent<OrbitalProjectile>();
            
            p.damage = damage;
            
            activeOrbitals.Add(newOrb);
        }
    }
}