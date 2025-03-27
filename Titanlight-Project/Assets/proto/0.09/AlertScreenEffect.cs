using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AlertScreenEffect : MonoBehaviour
{
    public static AlertScreenEffect Instance;

    [Header("Configurações do Overlay")]
    [Tooltip("CanvasGroup que contém o overlay vermelho.")]
    public CanvasGroup redOverlay;

    [Tooltip("Duração total do efeito (fade out do overlay).")]
    public float flashDuration = 1f;

    [Header("Configurações do Shake")]
    [Tooltip("Intensidade do tremor da câmera.")]
    public float shakeAmount = 5f;
    [Tooltip("Duração do tremor da câmera.")]
    public float shakeDuration = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Se desejar manter o objeto entre cenas, descomente a linha abaixo:
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inicia o efeito de alerta na tela com uma mensagem de descrição.
    /// </summary>
    /// <param name="eventDescription">Descrição do evento que pode ser exibida em log ou UI.</param>
    public void TriggerAlert(string eventDescription)
    {
        StartCoroutine(AlertSequence(eventDescription));
    }

    private IEnumerator AlertSequence(string eventDescription)
    {
        // Ativa o overlay vermelho
        if (redOverlay != null)
        {
            redOverlay.alpha = 1f;
            redOverlay.gameObject.SetActive(true);
        }

        // Efeito de shake na câmera
        Transform cam = Camera.main.transform;
        Vector3 originalPos = cam.position;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeAmount, shakeAmount) * 0.01f;
            float offsetY = Random.Range(-shakeAmount, shakeAmount) * 0.01f;
            cam.position = originalPos + new Vector3(offsetX, offsetY, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cam.position = originalPos;

        // Mantém o overlay por flashDuration e depois faz fade out
        elapsed = 0f;
        while (elapsed < flashDuration)
        {
            if (redOverlay != null)
                redOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (redOverlay != null)
        {
            redOverlay.alpha = 0f;
            redOverlay.gameObject.SetActive(false);
        }

        Debug.Log("Alerta: " + eventDescription);
    }
}
