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
        currentTime = startTime;
        timerRunning = true;
        if (timerText != null)
            originalColor = timerText.color; // Guarda a cor original
    }

    void Update()
    {
        if (timerRunning)
        {
            // Diminui o tempo a cada frame
            currentTime -= Time.deltaTime;

            if (currentTime <= 0f)
            {
                // Quando chega em zero
                currentTime = 0f;
                timerRunning = false;
                AtualizaTextoTimer(0f);
                IniciaInvasion(); // Chama o evento
            }
            else
            {
                AtualizaTextoTimer(currentTime); // Atualiza display
            }
        }
    }

    // Método para outros scripts tirarem tempo
    public void ReduceTime(float amount)
    {
        if (!timerRunning || currentTime <= 0f) return;

        currentTime -= amount;
        currentTime = Mathf.Max(currentTime, 0f); // Não deixa negativo
        AtualizaTextoTimer(currentTime);

        // Faz piscar vermelho
        StartCoroutine(FlashTimerRed());

        if (currentTime <= 0f)
        {
            timerRunning = false;
            IniciaInvasion();
        }
    }

    // Formata o tempo em minutos e segundos
    void AtualizaTextoTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Faz o timer piscar em vermelho
    IEnumerator FlashTimerRed()
    {
        if (timerText == null)
            yield break;

        timerText.color = Color.red; // Muda para vermelho
        yield return new WaitForSeconds(0.3f); // Espera um pouco
        timerText.color = originalColor; // Volta para cor normal
    }

    // Quando acaba o tempo
    void IniciaInvasion()
    {
        Debug.Log("Invasion iniciada!");
        Invasion.Invoke(); // Dispara eventos

        // Se tiver efeito de alerta na tela
        if (AlertScreenEffect.Instance != null)
        {
            AlertScreenEffect.Instance.TriggerAlert("O tempo acabou! Perigo iminente!");
        }
    }
}