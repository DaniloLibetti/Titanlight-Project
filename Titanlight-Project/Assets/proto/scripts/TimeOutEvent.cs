using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeOutEvent : MonoBehaviour
{
    public static TimeOutEvent Instance;

    public Image warningOverlay; // Arraste o painel vermelho no Inspector
    public float intensity = 0.1f; // Intensidade do tremor
    public float duration = 0.2f; // Duração de cada tremor
    public float heartbeatInterval = 1.5f; // Tempo entre batimentos

    private bool isEventActive = false;

    private void Awake()
    {
        Instance = this;
        warningOverlay.color = new Color(1, 0, 0, 0); // Começa invisível
    }

    public void StartTimeOutEvent()
    {
        if (!isEventActive)
        {
            isEventActive = true;
            StartCoroutine(WarningEffect());
            SoundManager.PlaySound(SoundType.WARNING);
        }
    }

    private IEnumerator WarningEffect()
    {
        while (isEventActive)
        {
            // Faz a tela piscar vermelha
            StartCoroutine(FadeOverlay(0.5f, 0.2f));

            // Tremor da tela
            Vector3 originalPos = Camera.main.transform.position;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                float offsetX = Random.Range(-intensity, intensity);
                float offsetY = Random.Range(-intensity, intensity);
                Camera.main.transform.position = originalPos + new Vector3(offsetX, offsetY, 0);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Camera.main.transform.position = originalPos; // Volta ao normal

            yield return new WaitForSeconds(heartbeatInterval);
        }
    }

    private IEnumerator FadeOverlay(float targetAlpha, float speed)
    {
        Color startColor = warningOverlay.color;
        float elapsedTime = 0;

        while (elapsedTime < speed)
        {
            float alpha = Mathf.Lerp(startColor.a, targetAlpha, elapsedTime / speed);
            warningOverlay.color = new Color(1, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f); // Pequeno delay

        elapsedTime = 0;
        while (elapsedTime < speed)
        {
            float alpha = Mathf.Lerp(targetAlpha, 0, elapsedTime / speed);
            warningOverlay.color = new Color(1, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
