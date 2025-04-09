using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    [Header("Configurações do Timer")]
    public float startTime = 60f; // Tempo inicial em segundos

    [Header("Componentes")]
    public TextMeshProUGUI timerText; // Onde mostra o tempo

    [Header("Eventos")]
    public UnityEvent Invasion; // O que acontece quando o tempo zera

    // Variáveis privadas
    private float currentTime; // Tempo atual
    private bool timerRunning = false; // Se o timer está contando
    private Color originalColor; // Cor normal do texto

    void Start()
    {
        // Se o timer estiver ativo ao iniciar a run, reinicia
        ResetTimer();
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
                IniciaInvasion(); // Evento quando o tempo zera
            }
            else
            {
                AtualizaTextoTimer(currentTime);
            }
        }
    }

    // Permite que outros scripts reduzam o tempo
    public void ReduceTime(float amount)
    {
        if (!timerRunning || currentTime <= 0f) return;

        currentTime -= amount;
        currentTime = Mathf.Max(currentTime, 0f);
        AtualizaTextoTimer(currentTime);

        StartCoroutine(FlashTimerRed());

        if (currentTime <= 0f)
        {
            timerRunning = false;
            IniciaInvasion();
        }
    }

    // Atualiza o display do timer
    void AtualizaTextoTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Faz o texto piscar em vermelho
    IEnumerator FlashTimerRed()
    {
        if (timerText == null)
            yield break;

        timerText.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        timerText.color = originalColor;
    }

    // Quando o tempo chega a zero
    void IniciaInvasion()
    {
        Debug.Log("Invasion iniciada!");
        Invasion.Invoke();

        if (AlertScreenEffect.Instance != null)
        {
            AlertScreenEffect.Instance.TriggerAlert("O tempo acabou! Perigo iminente!");
        }
    }

    // Método chamado quando o player morre para parar o timer e esconder o display
    public void OnPlayerDeath()
    {
        currentTime = startTime;
        timerRunning = false;
        AtualizaTextoTimer(currentTime);

        // Desativa o componente e, opcionalmente, o GameObject
        this.enabled = false;
        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    // Método para reiniciar e reativar o timer no início de cada run
    public void ResetTimer()
    {
        // Reativa o GameObject se estiver desativado
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // Reativa o componente e reinicia a contagem
        this.enabled = true;
        currentTime = startTime;
        timerRunning = true;
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            originalColor = timerText.color;
            AtualizaTextoTimer(currentTime);
        }
    }
}
