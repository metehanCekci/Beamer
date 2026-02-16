using UnityEngine;
using UnityEngine.UI; // Required for Slider class
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Health and Defense")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool canRegenerateHealth = false;
    public float healthRegenRate = 0f;

    [Header("Experience and Level")]
    public int currentLevel = 1;
    public long currentXP = 0;
    public long xpToNextLevel = 10;
    public float xpMultiplier = 1f; // XP Multiplier
    public float luck = 1f; // Luck multiplier (1 = 100% normal luck)
    public float magnetRange = 5f; // XP Pickup range
    public int killCount = 0;

    [Header("UI Connections")]
    public Slider healthSlider;
    public TMP_Text healthText; // 100/100 text
    public Slider xpSlider; // Level Slider
    public TMP_Text levelText; // Level text above minimap
    public TMP_Text timeText; // Top left timer
    public TMP_Text killText; // Top left kill count
    public GameObject gameOverPanel;
    public TMP_Text gameOverStatsText; // NEW: Game Over statistics

    [Header("Boss Settings")]
    public GameObject portalPrefab;
    public int killsToBoss = 100;
    private bool portalSpawned = false;

    private float initialSliderWidth;
    private float gameTime;
    private bool isInvincible = false;

    void Start()
    {
        // Apply Permanent Upgrades
        if (PersistentGameManager.Instance != null)
        {
            int healthLvl = PersistentGameManager.Instance.GetUpgradeLevel("Health");
            maxHealth += healthLvl * 10f; // +10 Health per level
        }

        currentHealth = maxHealth;

        // Load Session Data (if coming from another scene like Boss Fight)
        if (PersistentGameManager.Instance != null)
        {
            PersistentGameManager.Instance.LoadSessionData(this);
        }

        UpdateHealthUI();
        UpdateXPUI();

        // Set initial health bar value
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            healthSlider.interactable = false; // Prevent user interaction
        }
        if (xpSlider != null)
        {
            xpSlider.interactable = false; // Prevent user interaction
        }
    }

    public void SetInvincible(bool state)
    {
        isInvincible = state;
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount; 
        UpdateHealthUI();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float amount)
    {
        if (isInvincible) return;

        // Overload Reward: Take 50% more damage
        if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.Overload))
        {
            amount *= 1.5f;
        }

        currentHealth -= amount;
        UpdateHealthUI();

        // Screen Shake Trigger
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.2f, 0.3f); // Shake on damage
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Time.timeScale = 0f;
        gameObject.SetActive(false);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverStatsText != null)
            {
                int minutes = Mathf.FloorToInt(gameTime / 60F);
                int seconds = Mathf.FloorToInt(gameTime % 60F);
                string timeStr = $"{minutes:00}:{seconds:00}";
                
                gameOverStatsText.text = $"Time Survived: {timeStr}\nEnemies Killed: {killCount}\nLevel: {currentLevel}";
            }
        }
    }

    void Update()
    {
        // Time Counter
        gameTime += Time.deltaTime;
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60F);
            int seconds = Mathf.FloorToInt(gameTime % 60F);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }

        if (canRegenerateHealth && currentHealth < maxHealth)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            UpdateHealthUI();
        }
    }

    public void AddKill()
    {
        killCount++;
        if (killText != null)
        {
            killText.text = "Kills: " + killCount;
        }

        if (!portalSpawned && killCount >= killsToBoss)
        {
            SpawnPortal();
        }
    }

    void SpawnPortal()
    {
        portalSpawned = true;
        if (portalPrefab != null)
        {
            // Spawn portal near player (random direction)
            Vector3 randomDir = Random.insideUnitCircle.normalized * 5f;
            Vector3 spawnPos = transform.position + randomDir;
            
            Instantiate(portalPrefab, spawnPos, Quaternion.identity);
            Debug.Log("Boss Portal Spawned!");

            // Stop enemy spawning
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.StopSpawning();
            }
        }
    }

    public void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        if (healthText != null)
        {
            healthText.text = $"{(int)currentHealth}/{(int)maxHealth}";
        }
    }

    private void UpdateXPUI()
    {
        if (xpSlider != null)
        {
            xpSlider.maxValue = xpToNextLevel;
            xpSlider.value = currentXP;
        }
        if (levelText != null)
        {
            levelText.text = "Lvl " + currentLevel;
        }
    }

    // XP Gem tarafndan arlr
    public void AddXP(int amount)
    {
        currentXP += (int)(amount * xpMultiplier);
        UpdateXPUI();

        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = (long)(xpToNextLevel * 1.5f);
        UpdateXPUI();

        Debug.Log("LEVEL UP! New level: " + currentLevel);

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OpenPanel();
        }
    }
}