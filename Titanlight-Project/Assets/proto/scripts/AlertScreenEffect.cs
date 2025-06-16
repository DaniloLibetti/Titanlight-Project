using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AlertScreenEffect : MonoBehaviour
{
    public static AlertScreenEffect Instance;

    [Header("Configurações do Overlay")]
    [Tooltip("Canvas que contém a Image de overlay vermelho.")]
    public Canvas redOverlayCanvas;

    [Tooltip("Image dentro do Canvas que preenche toda a tela.")]
    public Image redOverlayImage;

    [Tooltip("Duração total do efeito (fade out do overlay).")]
    public float flashDuration = 1f;

    [Header("Configurações do Shake")]
    [Tooltip("Intensidade do tremor da câmera.")]
    public float shakeAmount = 5f;
    [Tooltip("Duração do tremor da câmera.")]
    public float shakeDuration = 0.5f;

    private Coroutine currentAlertCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); se necessário
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (redOverlayCanvas != null)
            redOverlayCanvas.enabled = false;

        if (redOverlayImage != null)
        {
            Color c = redOverlayImage.color;
            c.a = 0f;
            redOverlayImage.color = c;
        }
    }

    public void TriggerAlert(string eventDescription)
    {
        if (currentAlertCoroutine != null)
            StopCoroutine(currentAlertCoroutine);
        currentAlertCoroutine = StartCoroutine(AlertSequence(eventDescription));
    }

    private IEnumerator AlertSequence(string eventDescription)
    {
        if (redOverlayCanvas != null)
            redOverlayCanvas.enabled = true;

        if (redOverlayImage != null)
        {
            Color baseColor = redOverlayImage.color;
            baseColor.r = 1f; baseColor.g = 0f; baseColor.b = 0f; baseColor.a = 1f;
            redOverlayImage.color = baseColor;
        }

        if (Camera.main != null)
        {
            Transform cam = Camera.main.transform;
            Vector3 originalPos = cam.position;
            float elapsedShake = 0f;
            while (elapsedShake < shakeDuration)
            {
                float offsetX = Random.Range(-shakeAmount, shakeAmount) * 0.01f;
                float offsetY = Random.Range(-shakeAmount, shakeAmount) * 0.01f;
                cam.position = originalPos + new Vector3(offsetX, offsetY, 0);
                elapsedShake += Time.deltaTime;
                yield return null;
            }
            cam.position = originalPos;
        }

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            if (redOverlayImage != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
                Color c = redOverlayImage.color;
                c.a = alpha;
                redOverlayImage.color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (redOverlayImage != null)
        {
            Color c = redOverlayImage.color;
            c.a = 0f;
            redOverlayImage.color = c;
        }
        if (redOverlayCanvas != null)
            redOverlayCanvas.enabled = false;

        Debug.Log("Alerta: " + eventDescription);
        currentAlertCoroutine = null;
    }
}
