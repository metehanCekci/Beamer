using UnityEngine;

/// <summary>
/// Developer cheat codes for testing.
/// Only active in Unity Editor or Development builds.
/// </summary>
public class DeveloperCheats : MonoBehaviour
{
    [Header("Cheat Settings")]
    [SerializeField] private int coinCheatAmount = 500;
    [SerializeField] private bool enableCheats = true;

    void Update()
    {
        // Only work in Editor or Development builds
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!enableCheats) return;

        // M key - Add coins
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddCoins();
        }

        // K key - Reset coins to 0
        if (Input.GetKeyDown(KeyCode.K))
        {
            ResetCoins();
        }

        // L key - Unlock all characters
        if (Input.GetKeyDown(KeyCode.L))
        {
            UnlockAllCharacters();
        }
        #endif
    }

    void AddCoins()
    {
        if (PersistentGameManager.Instance != null)
        {
            PersistentGameManager.Instance.AddCoins(coinCheatAmount);
            Debug.Log($"[CHEAT] Added {coinCheatAmount} coins! Total: {PersistentGameManager.Instance.totalCoins}");

            // Update UI if MainMenuManager exists
            MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
            if (mainMenu != null)
            {
                mainMenu.UpdateUI();
            }
        }
        else
        {
            Debug.LogWarning("[CHEAT] PersistentGameManager not found!");
        }
    }

    void ResetCoins()
    {
        if (PersistentGameManager.Instance != null)
        {
            PersistentGameManager.Instance.totalCoins = 0;
            PersistentGameManager.Instance.SaveData();
            Debug.Log("[CHEAT] Coins reset to 0!");

            MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
            if (mainMenu != null)
            {
                mainMenu.UpdateUI();
            }
        }
    }

    void UnlockAllCharacters()
    {
        if (CharacterDatabase.Instance != null)
        {
            int count = CharacterDatabase.Instance.GetCharacterCount();
            for (int i = 0; i < count; i++)
            {
                CharacterData character = CharacterDatabase.Instance.GetCharacter(i);
                if (character != null && !character.isUnlocked)
                {
                    string unlockKey = $"Character_{i}_Unlocked";
                    PlayerPrefs.SetInt(unlockKey, 1);
                }
            }
            PlayerPrefs.Save();
            Debug.Log("[CHEAT] All characters unlocked!");
        }
    }

    // Display active cheats on screen
    void OnGUI()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!enableCheats) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 14;

        GUI.Label(new Rect(10, 10, 300, 80), 
            "DEVELOPER CHEATS:\n" +
            "M - Add 500 Coins\n" +
            "K - Reset Coins\n" +
            "L - Unlock All Characters", 
            style);
        #endif
    }
}
