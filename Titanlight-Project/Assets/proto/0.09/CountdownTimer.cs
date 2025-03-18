using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    [Header("Configura��es do Timer")]
    [Tooltip("Tempo inicial em segundos para a contagem regressiva.")]
    public float startTime = 60f;

    [Header("Componentes")]
    [Tooltip("Refer�ncia para o TextMeshProUGUI que exibir� a contagem.")]
    public TextMeshProUGUI timerText;

    [Header("Eventos")]
    [Tooltip("Evento que ser� invocado quando o tempo acabar.")]
    public UnityEvent Invasion;

    private float currentTime;
    private bool timerRunning = false;

    void Start()
    {
        currentTime = startTime;
        timerRunning = true;
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

    // M�todo p�blico para permitir que outros scripts reduzam o tempo
    public void ReduceTime(float amount)
    {
        if (!timerRunning || currentTime <= 0f) return;

        currentTime -= amount;
        currentTime = Mathf.Max(currentTime, 0f);
        AtualizaTextoTimer(currentTime);

        Debug.Log("Tempo reduzido em: -" + amount + "s");

        // Se o tempo zerar ap�s a redu��o, iniciar a invas�o imediatamente
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
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // M�todo chamado ao terminar a contagem para iniciar o evento Invasion
    void IniciaInvasion()
    {
        Debug.Log("Invasion iniciada!");
        Invasion.Invoke();
    }
}
