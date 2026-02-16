using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject upgradesPanel;
    public GameObject characterSelectPanel;

    [Header("UI Elemanları")]
    public TMP_Text coinsText;

    private void Start()
    {
        // Başlangıçta sadece ana menüyü göster
        ShowPanel(mainPanel);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (PersistentGameManager.Instance != null && coinsText != null)
        {
            coinsText.text = "Coins: " + PersistentGameManager.Instance.totalCoins;
        }
    }

    public void PlayGame()
    {
        // Open character selection instead of directly loading game
        // Player must select a character before starting
        OpenCharacterSelect();
    }

    public void StartGameWithSelectedCharacter()
    {
        // Called from CharacterSelectUI after character is selected
        // Load the first level scene
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game quit.");
    }

    public void OpenSettings()
    {
        ShowPanel(settingsPanel);
    }

    public void OpenUpgrades()
    {
        ShowPanel(upgradesPanel);
        UpdateUI();
    }

    public void OpenCharacterSelect()
    {
        ShowPanel(characterSelectPanel);
    }

    public void BackToMain()
    {
        ShowPanel(mainPanel);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if(mainPanel) mainPanel.SetActive(false);
        if(settingsPanel) settingsPanel.SetActive(false);
        if(upgradesPanel) upgradesPanel.SetActive(false);
        if(characterSelectPanel) characterSelectPanel.SetActive(false);

        if(panelToShow) panelToShow.SetActive(true);
        
        UpdateUI();
    }
    
    // Kalıcı Upgrade Satın Alma Örneği (Butona bağlanacak)
    public void BuyUpgrade(string upgradeKey)
    {
        int currentLevel = PersistentGameManager.Instance.GetUpgradeLevel(upgradeKey);
        int cost = (currentLevel + 1) * 100; // Örnek maliyet formülü

        if (PersistentGameManager.Instance.SpendCoins(cost))
        {
            PersistentGameManager.Instance.SetUpgradeLevel(upgradeKey, currentLevel + 1);
            Debug.Log($"{upgradeKey} upgraded! New Level: {currentLevel + 1}");
            UpdateUI();
        }
        else
        {
            Debug.Log("Insufficient funds!");
        }
    }
}
