using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Mermi baktığı yöne doğru gider
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();
            if (player != null)
            {
                // PlayerStats'a TakeDamage metodu eklememiz gerekebilir, 
                // şimdilik canı doğrudan azaltalım veya varsa metodu kullanalım.
                // PlayerStats'ı kontrol ettim, TakeDamage yok, currentHealth public.
                
                player.currentHealth -= damage;
                player.UpdateHealthUI(); // UI güncellemesi
                
                // Eğer can 0'ın altına düşerse
                if (player.currentHealth <= 0)
                {
                    // Ölüm mantığı PlayerStats veya GameManager'da olmalı
                    // Şimdilik basitçe konsola yazalım
                    Debug.Log("Player Died!");
                    // Burada PlayerStats içindeki ölüm fonksiyonunu çağırabiliriz
                }
            }
            Destroy(gameObject);
        }
        // else if (collision.CompareTag("Obstacle")) // Tag hatası vermemesi için kapatıldı
        // {
        //    Destroy(gameObject);
        // }
    }
}
