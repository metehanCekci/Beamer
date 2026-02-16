using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public PlayerStats playerStats;
    public Slider slider;
    public Vector3 offset = new Vector3(0, -1.5f, 0); // Karakterin altı için offset

    void Update()
    {
        if (playerStats != null && slider != null)
        {
            slider.maxValue = playerStats.maxHealth;
            slider.value = playerStats.currentHealth;
            
            // Pozisyonu güncelle (Karakteri takip et)
            // Z eksenini -1 yaparak diğer objelerin önünde görünmesini sağla
            Vector3 targetPos = playerStats.transform.position + offset;
            targetPos.z = -1f; 
            transform.position = targetPos;
        }
    }
}
