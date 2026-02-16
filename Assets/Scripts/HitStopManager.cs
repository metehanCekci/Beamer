using UnityEngine;
using System.Collections;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }

    private bool isStopped = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Stop(float duration)
    {
        if (isStopped) return;
        StartCoroutine(StopRoutine(duration));
    }

    IEnumerator StopRoutine(float duration)
    {
        isStopped = true;
        float originalTimeScale = Time.timeScale;
        
        // Eğer oyun zaten duraklatılmışsa (Level Up ekranı vb.) hit stop yapma
        if (originalTimeScale == 0) 
        {
            isStopped = false;
            yield break;
        }

        Time.timeScale = 0.9f; // Freeze game completely
        
        yield return new WaitForSecondsRealtime(duration);

        // Check if game is paused by UI (Level Up or Pause Menu)
        bool isPausedByUI = false;

        if (UpgradeManager.Instance != null && UpgradeManager.Instance.levelUpPanel.activeSelf)
        {
            isPausedByUI = true;
        }

        if (PauseManager.Instance != null && PauseManager.Instance.pausePanel.activeSelf)
        {
            isPausedByUI = true;
        }

        // Only restore time if NOT paused by UI
        if (!isPausedByUI)
        {
            Time.timeScale = originalTimeScale;
        }
        
        isStopped = false;
    }
}
