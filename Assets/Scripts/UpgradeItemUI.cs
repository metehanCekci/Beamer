using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    [Header("Settings")]
    public string upgradeKey; // Ex: "Health", "Damage"
    public string displayName; // Ex: "Max Health"
    public int baseCost = 100; // Base cost
    
    [Header("UI Connections")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text costText;
    public Button buyButton;

    void Start()
    {
        // If button is not assigned, search in children
        if (buyButton == null)
        {
            buyButton = GetComponentInChildren<Button>();
        }

        if(buyButton != null)
        {
            // Clear previous listeners (to prevent double clicking)
            buyButton.onClick.RemoveListener(Buy);
            // Bind click event via code
            buyButton.onClick.AddListener(Buy);
        }
        else
        {
            Debug.LogError($"Button not found on {gameObject.name}!");
        }

        UpdateUI();
    }

    void OnEnable()
    {
        UpdateUI();
    }

    void Update()
    {
        // For testing convenience: update button state instantly if money changes in Inspector
        if (buyButton != null && PersistentGameManager.Instance != null)
        {
            int currentLevel = PersistentGameManager.Instance.GetUpgradeLevel(upgradeKey);
            int cost = CalculateCost(currentLevel);
            bool canAfford = PersistentGameManager.Instance.totalCoins >= cost;
            
            if (buyButton.interactable != canAfford)
            {
                buyButton.interactable = canAfford;
            }
        }
    }

    public void Buy()
    {
        if (PersistentGameManager.Instance == null) return;

        int currentLevel = PersistentGameManager.Instance.GetUpgradeLevel(upgradeKey);
        int cost = CalculateCost(currentLevel);

        if (PersistentGameManager.Instance.SpendCoins(cost))
        {
            PersistentGameManager.Instance.SetUpgradeLevel(upgradeKey, currentLevel + 1);
            UpdateUI();
            
            // Update coin amount in main menu
            // Using old method for Unity version compatibility, ignore warnings
            MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
            if (mainMenu != null) mainMenu.UpdateUI();
            
            Debug.Log($"{displayName} upgraded! New Level: {currentLevel + 1}");
        }
    }

    public void UpdateUI()
    {
        if (PersistentGameManager.Instance == null) return;

        int currentLevel = PersistentGameManager.Instance.GetUpgradeLevel(upgradeKey);
        int cost = CalculateCost(currentLevel);

        if(nameText) nameText.text = displayName;
        if(levelText) levelText.text = $"Lvl {currentLevel}";
        if(costText) costText.text = $"{cost} Gold";

        if(buyButton)
        {
            // Button active if enough money, passive if not
            buyButton.interactable = PersistentGameManager.Instance.totalCoins >= cost;
        }
    }

    int CalculateCost(int level)
    {
        // Maliyet formülü: BaseCost * (Level + 1)
        // Örn: 100, 200, 300...
        return baseCost * (level + 1);
    }
}
