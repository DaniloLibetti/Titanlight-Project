using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    [Header("Configurações do Timer")]
    [Tooltip("Tempo inicial em segundos para a contagem regressiva.")]
    public float startTime = 60f;

    [Header("Componentes")]
    [Tooltip("Referência para o TextMeshProUGUI que exibirá a contagem.")]
    public TextMeshProUGUI timerText;

    [Header("Eventos")]
    [Tooltip("Evento que será invocado quando o tempo acabar.")]
    public UnityEvent Invasion;

    private float currentTime;
    private bool timerRunning = false;
    private Color originalColor;

    void Start()
    {
        currentTime = startTime;
        timerRunning = true;
        if (timerText != null)
            originalColor = timerText.color;
    }

    void Update()
    {
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                timerRunning = false;
                AtualizaTextoTimer(0f);
                IniciaInvasion();
            }
            else
            {
                AtualizaTextoTimer(currentTime);
            }
        }
    }

    // Método público para permitir que outros scripts reduzam o tempo
    public void ReduceTime(float amount)
    {
        if (!timerRunning || currentTime <= 0f) return;

        currentTime -= amount;
        currentTime = Mathf.Max(currentTime, 0f);
        AtualizaTextoTimer(currentTime);
        Debug.Log("Tempo reduzido em: -" + amount + "s");

        // Flash vermelho no timer
        StartCoroutine(FlashTimerRed());

        // Se o tempo zerar após a redução, iniciar o evento
        if (currentTime <= 0f)
        {
            timerRunning = false;
            IniciaInvasion();
        }
    }

    // Atualiza o componente TextMeshPro com o tempo formatado (MM:SS)
    void AtualizaTextoTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Coroutine para piscar o timer em vermelho
    IEnumerator FlashTimerRed()
    {
        if (timerText == null)
            yield break;

        timerText.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        timerText.color = originalColor;
    }

    // Método chamado ao terminar a contagem para iniciar o evento Invasion
    void IniciaInvasion()
    {
        Debug.Log("Invasion iniciada!");
        Invasion.Invoke();
        // Chama o efeito de alerta (caso exista na cena)
        if (AlertScreenEffect.Instance != null)
        {
            AlertScreenEffect.Instance.TriggerAlert("O tempo acabou! Perigo iminente!");
        }
    }
}
