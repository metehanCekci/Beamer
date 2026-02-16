using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CameraController cameraController;
    private float shakeTimer;
    private float shakeMagnitude;
    private float shakeFadeTime;

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

    void Start()
    {
        cameraController = GetComponent<CameraController>();
    }

    void Update()
    {
        if (cameraController == null) return;

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.unscaledDeltaTime;

            float xAmount = Random.Range(-1f, 1f) * shakeMagnitude;
            float yAmount = Random.Range(-1f, 1f) * shakeMagnitude;

            cameraController.shakeOffset = new Vector3(xAmount, yAmount, 0f);

            // Efektin yavaşça azalması için
            shakeMagnitude = Mathf.MoveTowards(shakeMagnitude, 0f, shakeFadeTime * Time.unscaledDeltaTime);
        }
        else
        {
            cameraController.shakeOffset = Vector3.zero;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeMagnitude = magnitude;
        shakeFadeTime = magnitude / duration;
    }
}
