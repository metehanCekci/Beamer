using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        
        // TROUBLESHOOTER: Force reset settings
        if (textMesh != null)
        {
            // Center alignment
            textMesh.alignment = TextAlignmentOptions.Center;
            
            // Reset margins
            textMesh.margin = new Vector4(0, 0, 0, 0);
            
            // Fix RectTransform settings
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.pivot = new Vector2(0.5f, 0.5f); // Set pivot to center
                rect.anchoredPosition = Vector2.zero; // Reset position
                rect.sizeDelta = new Vector2(5f, 2f); // Set reasonable size
            }
        }
    }

    private Transform targetTransform;
    private Vector3 relativePosition;

    public void Setup(int damageAmount, Transform target, bool isCritical)
    {
        this.targetTransform = target;
        textMesh.SetText(damageAmount.ToString());
        
        if (isCritical)
        {
            textMesh.fontSize = 6; // Bigger font
            textMesh.color = Color.yellow; // Critical color
            textColor = Color.yellow;
        }
        else
        {
            textMesh.fontSize = 4; // Normal font
            textMesh.color = Color.white; // Normal color
            textColor = Color.white;
        }

        disappearTimer = 1f;
        
        if (targetTransform != null)
        {
            // 180 derece ark (Üst yarım daire: Sol(180) -> Sağ(0))
            // Rastgele açı ve mesafe
            float randomAngle = Random.Range(0f, 180f);
            float distance = Random.Range(1.2f, 2.0f);

            float angleRad = randomAngle * Mathf.Deg2Rad;
            // Offset hesapla (Z ekseninde -2 vererek kameraya yaklaştırıyoruz)
            relativePosition = new Vector3(Mathf.Cos(angleRad) * distance, Mathf.Sin(angleRad) * distance, -2f);
        }
        else
        {
            relativePosition = Vector3.zero;
        }

        // Yukarı doğru süzülme hareketi
        moveVector = new Vector3(0, 1f) * 3f;
    }

    private void Update()
    {
        if (targetTransform != null)
        {
            // Hedef varsa: Hedefe göre konumlan
            relativePosition += moveVector * Time.deltaTime;
            transform.position = targetTransform.position + relativePosition;
        }
        else
        {
            // Hedef yoksa: Olduğu yerden hareket et
            transform.position += moveVector * Time.deltaTime;
        }

        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > 0.5f)
        {
            // İlk yarısında büyüsün
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            // İkinci yarısında küçülsün
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            // Yavaşça kaybol
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
