using UnityEngine;

public class MagnetPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Find all XP Gems in the scene
            // Using FindObjectsByType for newer Unity versions (faster)
            XPGem[] allGems = Object.FindObjectsByType<XPGem>(FindObjectsSortMode.None);
            
            foreach (XPGem gem in allGems)
            {
                gem.Magnetize();
            }

            Debug.Log("Magnet Activated! " + allGems.Length + " gems collected.");
            
            // Destroy the magnet object
            Destroy(gameObject);
        }
    }
}
