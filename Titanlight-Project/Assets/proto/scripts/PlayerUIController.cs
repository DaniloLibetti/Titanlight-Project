using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [Header("Elementos de UI")]
    [SerializeField] private Slider healthSlider;    // Slider para a vida
    [SerializeField] private Slider dashSlider;      // Slider para o dash (cooldown)
    [SerializeField] private Slider chargeSlider;    // Slider para o tiro carregado
    [SerializeField] private Slider heatSlider;      // Slider para o sistema de aquecimento

    [Header("Referência ao Player")]
    [SerializeField] private PlayerController playerController; // Referência ao script do player

    void Start()
    {
        // Configura o valor máximo do slider de vida
        if (playerController != null && healthSlider != null)
        {
            healthSlider.maxValue = playerController.GetComponent<Health>().MaxHealth;
        }
    }

    void Update()
    {
        if (playerController != null)
        {
            if (healthSlider != null)
            {
                healthSlider.value = playerController.GetComponent<Health>().CurrentHealth;
            }
            if (dashSlider != null)
            {
                dashSlider.value = playerController.DashProgress;
            }
            if (chargeSlider != null)
            {
                chargeSlider.value = playerController.ChargeProgress;
            }
            if (heatSlider != null)
            {
                heatSlider.value = playerController.HeatProgress;
            }
        }
    }
}
