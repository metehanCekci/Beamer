using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // Takip edilecek hedef (Bizim Player)
    public float smoothSpeed = 0.125f; // Takip yumuakl (0 ile 1 aras)
    public Vector3 offset; // Kamera ile oyuncu arasndaki mesafe fark (Z ekseni iin art)
    
    [HideInInspector]
    public Vector3 shakeOffset; // CameraShake tarafndan ayarlanacak

    void LateUpdate() // Kamera ilemleri her zaman LateUpdate'te yaplr
    {
        if (target == null) return;

        // Kamerann gitmesi gereken hedef pozisyon (Sadece X ve Y, Z sabit kalmal)
        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, target.position.y + offset.y, -10f);

        // Mevcut pozisyondan hedef pozisyona yumuak gei (Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Shake efektini ekle
        transform.position = smoothedPosition + shakeOffset;
    }
}