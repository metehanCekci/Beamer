using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Character selection screen UI controller.
/// Displays available characters and handles selection.
/// </summary>
public class CharacterSelectUI : MonoBehaviour
{
    [Header("UI References - Required")]
    [Tooltip("Beamer ve Vity butonlarının bulunduğu Text (TMP)")]
    public TMP_Text beamerButtonText;
    
    [Tooltip("Beamer ve Vity butonlarının bulunduğu Text (TMP)")]
    public TMP_Text vityButtonText;

    [Header("UI References - Optional")]
    public TMP_Text characterDescriptionText;
    public Image characterPortraitImage;
    public TMP_Text statsText;
    public TMP_Text coinDisplayText;
    public TMP_Text unlockCostText;

    [Header("Buttons")]
    public Button beamerButton;
    public Button vityButton;
    public Button playButton;
    public Button unlockButton;

    [Header("Settings")]
    public string gameplaySceneName = "Level1";

    private int currentIndex = 0;
    private CharacterData currentCharacter;

    void Start()
    {
        // Load last selected character
        if (PersistentGameManager.Instance != null)
        {
            currentIndex = PersistentGameManager.Instance.selectedCharacterIndex;
        }

        // Setup character selection buttons
        if (beamerButton != null)
            beamerButton.onClick.AddListener(() => SelectCharacter(0));
        
        if (vityButton != null)
            vityButton.onClick.AddListener(() => SelectCharacter(1));
        
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);
        
        if (unlockButton != null)
            unlockButton.onClick.AddListener(OnUnlockButtonClicked);

        UpdateDisplay();
    }

    void SelectCharacter(int index)
    {
        currentIndex = index;
        Debug.Log($"[CharacterSelect] Selected index: {index}");
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (CharacterDatabase.Instance == null)
        {
            Debug.LogError("CharacterDatabase not found!");
            return;
        }

        currentCharacter = CharacterDatabase.Instance.GetCharacter(currentIndex);
        if (currentCharacter == null) return;

        bool isUnlocked = CharacterDatabase.Instance.IsCharacterUnlocked(currentIndex);

        // Update button texts to show selection
        if (beamerButtonText != null)
        {
            beamerButtonText.text = currentIndex == 0 ? "> Beamer <" : "Beamer";
        }

        if (vityButtonText != null)
        {
            vityButtonText.text = currentIndex == 1 ? "> Vity <" : "Vity";
        }

        if (characterDescriptionText != null)
            characterDescriptionText.text = currentCharacter.description;

        if (characterPortraitImage != null && currentCharacter.characterPortrait != null)
            characterPortraitImage.sprite = currentCharacter.characterPortrait;

        // Update stats
        if (statsText != null)
        {
            statsText.text = $"HP: {currentCharacter.baseMaxHealth}\n" +
                           $"Speed: {currentCharacter.baseMoveSpeed}\n" +
                           $"Damage: {currentCharacter.baseDamage}\n" +
                           $"Fire Rate: {currentCharacter.baseFireRate:F2}s";
        }

        // Update coins display
        if (coinDisplayText != null && PersistentGameManager.Instance != null)
        {
            coinDisplayText.text = $"Coins: {PersistentGameManager.Instance.totalCoins}";
        }

        // Show/Hide unlock button
        if (unlockButton != null)
        {
            unlockButton.gameObject.SetActive(!isUnlocked);
            if (!isUnlocked && unlockCostText != null)
            {
                unlockCostText.text = $"Unlock: {currentCharacter.unlockCost} Coins";
            }
        }

        // Enable/Disable play button
        if (playButton != null)
        {
            playButton.interactable = isUnlocked;
        }
    }

    void OnUnlockButtonClicked()
    {
        if (CharacterDatabase.Instance != null)
        {
            if (CharacterDatabase.Instance.UnlockCharacter(currentIndex))
            {
                UpdateDisplay();
            }
        }
    }

    void OnPlayButtonClicked()
    {
        // Save selected character
        if (PersistentGameManager.Instance != null)
        {
            PersistentGameManager.Instance.selectedCharacterIndex = currentIndex;
            PersistentGameManager.Instance.SaveData();
            Debug.Log($"[CharacterSelect] Saved character index: {currentIndex}");
        }
        else
        {
            Debug.LogError("[CharacterSelect] PersistentGameManager is NULL!");
        }

        // Try to use MainMenuManager if available, otherwise load scene directly
        MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
        if (mainMenu != null)
        {
            mainMenu.StartGameWithSelectedCharacter();
        }
        else
        {
            // Fallback: Load scene directly
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
