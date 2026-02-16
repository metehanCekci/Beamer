using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central database that holds all playable characters.
/// Singleton pattern for easy access throughout the game.
/// </summary>
public class CharacterDatabase : MonoBehaviour
{
    public static CharacterDatabase Instance { get; private set; }

    [Header("All Playable Characters")]
    [Tooltip("Add character ScriptableObjects here in order. Index 0 = Beamer, 1 = Vity, etc.")]
    public List<CharacterData> allCharacters = new List<CharacterData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Get character data by index
    /// </summary>
    public CharacterData GetCharacter(int index)
    {
        if (index >= 0 && index < allCharacters.Count)
        {
            CharacterData character = allCharacters[index];
            Debug.Log($"[CharacterDatabase] Returning character at index {index}: {(character != null ? character.characterName : "NULL")}");
            return character;
        }
        
        Debug.LogWarning($"Character index {index} out of range! Total characters: {allCharacters.Count}. Returning first character.");
        return allCharacters.Count > 0 ? allCharacters[0] : null;
    }

    /// <summary>
    /// Get currently selected character from PersistentGameManager
    /// </summary>
    public CharacterData GetCurrentCharacter()
    {
        if (PersistentGameManager.Instance != null)
        {
            int selectedIndex = PersistentGameManager.Instance.selectedCharacterIndex;
            Debug.Log($"[CharacterDatabase] Getting character at index: {selectedIndex}");
            return GetCharacter(selectedIndex);
        }
        
        Debug.LogWarning("[CharacterDatabase] PersistentGameManager is NULL, returning default (index 0)");
        return GetCharacter(0); // Default to Beamer
    }

    /// <summary>
    /// Check if a character is unlocked
    /// </summary>
    public bool IsCharacterUnlocked(int index)
    {
        CharacterData character = GetCharacter(index);
        if (character == null) return false;

        // Check PlayerPrefs for unlock status
        if (character.isUnlocked) return true;
        
        string unlockKey = $"Character_{index}_Unlocked";
        return PlayerPrefs.GetInt(unlockKey, 0) == 1;
    }

    /// <summary>
    /// Unlock a character (spend coins)
    /// </summary>
    public bool UnlockCharacter(int index)
    {
        CharacterData character = GetCharacter(index);
        if (character == null) return false;
        if (IsCharacterUnlocked(index)) return true; // Already unlocked

        // Check if player has enough coins
        if (PersistentGameManager.Instance != null)
        {
            if (PersistentGameManager.Instance.totalCoins >= character.unlockCost)
            {
                PersistentGameManager.Instance.totalCoins -= character.unlockCost;
                PersistentGameManager.Instance.SaveData();

                // Save unlock status
                string unlockKey = $"Character_{index}_Unlocked";
                PlayerPrefs.SetInt(unlockKey, 1);
                PlayerPrefs.Save();

                Debug.Log($"{character.characterName} unlocked!");
                return true;
            }
            else
            {
                Debug.Log("Not enough coins!");
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// Get total number of characters
    /// </summary>
    public int GetCharacterCount()
    {
        return allCharacters.Count;
    }
}
