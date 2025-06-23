using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class CountdownTimer : MonoBehaviour
{
    [Header("Configurações do Timer")]
    public float startTime = 60f; 

    [Header("Componentes")]
    public TextMeshProUGUI timerText; 
    [Header("Eventos")]
    public UnityEvent Invasion; 
    
    private float currentTime;
    private bool timerRunning = false; 
    private Color originalColor; 

    void Start()
    {
        
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
                UpdateTimerText(0f);
                StartInvasion(); 
            }
            else
            {
                UpdateTimerText(currentTime);
            }
        }
    }

   
    public void ReduceTime(float amount)
    {
        if (!timerRunning || currentTime <= 0f) return;

        currentTime -= amount;
        currentTime = Mathf.Max(currentTime, 0f);
        UpdateTimerText(currentTime);

        StartCoroutine(FlashTimerRed());

        if (currentTime <= 0f)
        {
            timerRunning = false;
            StartInvasion();
        }
    }

   
    void UpdateTimerText(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        if (timerText != null)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    
    IEnumerator FlashTimerRed()
    {
        if (timerText == null)
            yield break;

        timerText.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        timerText.color = originalColor;
    }

    
    void StartInvasion()
    {
        Debug.Log("Invasion iniciada!");
        Invasion.Invoke();

        if (AlertScreenEffect.Instance != null)
        {
            AlertScreenEffect.Instance.TriggerAlert("O tempo acabou! Perigo iminente!");
        }
    }

   
    public void ResetTimer()
    {
        
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        
        enabled = true;
        currentTime = startTime;
        timerRunning = true;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
            UpdateTimerText(currentTime);
        }
    }

    
    public void OnPlayerDeath()
    {
        currentTime = startTime;
        timerRunning = false;
        UpdateTimerText(currentTime);

        
        enabled = false;
        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }
}