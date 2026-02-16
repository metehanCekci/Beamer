using UnityEngine;
using System.Collections.Generic;

public class PersistentGameManager : MonoBehaviour
{
    public static PersistentGameManager Instance { get; private set; }

    [Header("Player Data")]
    public int totalCoins;
    public int selectedCharacterIndex = 0;
    
    // Permanent Upgrade Levels (UpgradeKey -> Level)
    // Example Keys: "Health", "Damage", "Speed", "Greed"
    public Dictionary<string, int> permanentUpgradeLevels = new Dictionary<string, int>();

    [Header("Session Data (For Scene Transitions)")]
    public float sessionMaxHealth = -1;
    public float sessionCurrentHealth = -1;
    public int sessionLevel = 1;
    public long sessionXP = 0;
    public int sessionKillCount = 0;
    public int sessionOrbitalCount = 0;
    public float sessionOrbitalDamage = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("TotalCoins", totalCoins);
        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacterIndex);
        
        // Dictionary'i kaydet
        foreach(var kvp in permanentUpgradeLevels)
        {
            PlayerPrefs.SetInt("PermUpgrade_" + kvp.Key, kvp.Value);
        }

        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);

        // Desteklenen kalıcı geliştirmeleri yükle
        string[] keys = new string[] { "Health" };
        
        foreach(var key in keys)
        {
            LoadUpgradeLevel(key);
        }
    }

    private void LoadUpgradeLevel(string key)
    {
        if (!permanentUpgradeLevels.ContainsKey(key))
        {
            permanentUpgradeLevels[key] = 0;
        }
        permanentUpgradeLevels[key] = PlayerPrefs.GetInt("PermUpgrade_" + key, 0);
    }

    public int GetUpgradeLevel(string key)
    {
        if (permanentUpgradeLevels.ContainsKey(key)) return permanentUpgradeLevels[key];
        return 0;
    }

    public void SetUpgradeLevel(string key, int level)
    {
        permanentUpgradeLevels[key] = level;
        SaveData();
    }
    
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        SaveData();
    }
    
    public bool SpendCoins(int amount)
    {
        if (totalCoins >= amount)
        {
            totalCoins -= amount;
            SaveData();
            return true;
        }
        return false;
    }

    [ContextMenu("Add 1000 Coins")]
    public void AddTestCoins()
    {
        AddCoins(1000);
        Debug.Log("1000 Coins added. Total: " + totalCoins);
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        totalCoins = 0;
        permanentUpgradeLevels.Clear();
        SaveData();
        Debug.Log("Data reset.");
    }

    public void SaveSessionData(PlayerStats stats)
    {
        sessionMaxHealth = stats.maxHealth;
        sessionCurrentHealth = stats.currentHealth;
        sessionLevel = stats.currentLevel;
        sessionXP = stats.currentXP;
        sessionKillCount = stats.killCount;

        OrbitalWeapon orbital = stats.GetComponent<OrbitalWeapon>();
        if (orbital != null)
        {
            sessionOrbitalCount = orbital.orbitalCount;
            sessionOrbitalDamage = orbital.damage;
        }
    }

    public void LoadSessionData(PlayerStats stats)
    {
        if (sessionMaxHealth > 0)
        {
            stats.maxHealth = sessionMaxHealth;
            stats.currentHealth = sessionCurrentHealth;
            stats.currentLevel = sessionLevel;
            stats.currentXP = sessionXP;
            stats.killCount = sessionKillCount;
        }

        OrbitalWeapon orbital = stats.GetComponent<OrbitalWeapon>();
        if (orbital != null && sessionOrbitalCount > 0)
        {
            orbital.SetStats(sessionOrbitalCount, sessionOrbitalDamage);
        }
    }
}
