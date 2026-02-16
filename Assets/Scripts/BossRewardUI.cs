using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BossRewardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject rewardPanel;
    public Button[] rewardButtons; // Assign 3 buttons
    
    // Tek Text yapısı (UpgradeManager ile uyumlu olması için)
    public TMP_Text[] rewardTexts; 

    private List<BossRewardType> currentOptions;

    public void ShowRewards()
    {
        if (BossRewardManager.Instance == null) return;

        currentOptions = BossRewardManager.Instance.GetRandomRewards(3);
        
        rewardPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game

        for (int i = 0; i < rewardButtons.Length; i++)
        {
            if (i < currentOptions.Count)
            {
                rewardButtons[i].gameObject.SetActive(true);
                BossRewardType reward = currentOptions[i];
                string description = BossRewardManager.Instance.GetRewardDescription(reward);

                // UpgradeManager ile aynı formatlama
                string titleText = $"<size=120%><b>{reward}</b></size>";
                string descText = $"<size=60%>{description}</size>";
                string fullText = $"{titleText}\n\n{descText}";

                if (i < rewardTexts.Length && rewardTexts[i] != null)
                {
                    rewardTexts[i].text = fullText;
                }

                // Remove old listeners
                rewardButtons[i].onClick.RemoveAllListeners();
                
                // Add new listener
                int index = i; // Capture index for closure
                rewardButtons[i].onClick.AddListener(() => OnRewardSelected(index));
            }
            else
            {
                rewardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnRewardSelected(int index)
    {
        if (index >= 0 && index < currentOptions.Count)
        {
            BossRewardType selected = currentOptions[index];
            BossRewardManager.Instance.AddReward(selected);
            Debug.Log("Selected Reward: " + selected);

            // Save Game Data before transition
            SaveGame();

            // Load Next Level
            LoadNextLevel();
        }
    }

    private void SaveGame()
    {
        if (PersistentGameManager.Instance != null)
        {
            // Find player to save stats
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    PersistentGameManager.Instance.SaveSessionData(stats);
                }
            }
        }
    }

    private void LoadNextLevel()
    {
        Time.timeScale = 1f; // Unpause
        SceneManager.LoadScene("Level2");
    }
}
