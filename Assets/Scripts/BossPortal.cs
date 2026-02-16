using UnityEngine;
using UnityEngine.SceneManagement;

public class BossPortal : MonoBehaviour
{
    public string bossSceneName = "Boss1";
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            isActivated = true;
            Debug.Log("Entering Boss Portal...");
            
            // Save current session state
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null && PersistentGameManager.Instance != null)
            {
                PersistentGameManager.Instance.SaveSessionData(stats);
            }
            
            SceneManager.LoadScene(bossSceneName);
        }
    }
}
