using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    // Singleton Addition
    public static EnemySpawner Instance { get; private set; }

    [System.Serializable]
    public class EnemyType
    {
        public string tag;
        public float weight;
        public float healthMultiplier;
        public int xpAmount = 1;
    }

    [Header("Settings")]
    public List<EnemyType> enemyTypes;
    public WeaponData weaponData;
    public float spawnRate = 2f;
    public float spawnDistance = 15f;

    [Header("XP Settings")]
    private const string XPGemTag = "XPGem";

    private Transform player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRate);
            try
            {
                SpawnEnemy();
            }
            catch (System.Exception e)
            {
                Debug.LogError("SpawnEnemy Hatası: " + e.Message);
            }
        }
    }

    public void StopSpawning()
    {
        StopAllCoroutines();
    }

    void SpawnEnemy()
    {
        if (player == null) return;

        // ObjectPooler yoksa veya yok edildiyse işlemi durdur
        if (ObjectPooler.Instance == null)
        {
            return;
        }

        if (enemyTypes == null || enemyTypes.Count == 0)
        {
             // Fallback
             Vector3 fallbackPos = player.position + (Vector3)(Random.insideUnitCircle.normalized * spawnDistance);
             ObjectPooler.Instance.SpawnFromPool("Enemy", fallbackPos, Quaternion.identity);
             return;
        }

        // Weighted Random Selection
        float totalWeight = 0f;
        foreach (var type in enemyTypes) totalWeight += type.weight;

        float randomValue = Random.Range(0, totalWeight);
        float currentWeight = 0f;
        EnemyType selectedType = null;

        foreach (var type in enemyTypes)
        {
            currentWeight += type.weight;
            if (randomValue <= currentWeight)
            {
                selectedType = type;
                break;
            }
        }

        if (selectedType == null) selectedType = enemyTypes[0];

        Vector3 randomDirection = Random.insideUnitCircle.normalized * spawnDistance;
        Vector3 spawnPosition = player.position + randomDirection;

        GameObject enemy = ObjectPooler.Instance.SpawnFromPool(selectedType.tag, spawnPosition, Quaternion.identity);
        
        if (enemy != null)
        {
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null && weaponData != null)
            {
                // Debug.Log($"Spawning {selectedType.tag} with Multiplier: {selectedType.healthMultiplier} and XP: {selectedType.xpAmount}");
                ai.SetMaxHealth(weaponData.BaseDamage * selectedType.healthMultiplier);
                ai.xpDropAmount = selectedType.xpAmount;
            }
            else
            {
                // Debug.LogError($"Failed to configure enemy {selectedType.tag}. AI: {ai != null}, WeaponData: {weaponData != null}");
            }
        }
        else
        {
            // Debug.LogError($"Failed to spawn enemy with tag: {selectedType.tag}");
        }
    }

    // Projectile.cs taraf�ndan �a�r�l�r
    public void DropXP(Vector3 position)
    {
        if (ObjectPooler.Instance == null) return;

        // G�NCELLEND�: Instantiate yerine Pooler kullan
        ObjectPooler.Instance.SpawnFromPool(XPGemTag, position, Quaternion.identity);
    }
}