using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Adiciona essa linha pra usar o TextMesh Pro

public class PlayerUIController : MonoBehaviour
{
    [Header("Elementos de UI (Serão buscados se não atribuídos no inspetor)")]
    [SerializeField] private Slider healthSlider;    // Slider para a vida
    [SerializeField] private Slider dashSlider;      // Slider para o dash (cooldown)
    [SerializeField] private Slider chargeSlider;    // Slider para o tiro carregado
    [SerializeField] private Slider heatSlider;      // Slider para o sistema de aquecimento

    [Header("Contador de Moedas")]
    // Alterei o tipo de Text para TMP_Text para usar o TextMesh Pro
    [SerializeField] private TMP_Text coinText;

    [Header("Referência ao Player")]
    [SerializeField] private PlayerController playerController; // Referência ao script do Player

    void Awake()
    {
        // Busca os sliders se não estiverem atribuídos via inspetor
        if (healthSlider == null)
        {
            GameObject healthObj = GameObject.Find("HealthSlider");
            if (healthObj != null)
                healthSlider = healthObj.GetComponent<Slider>();
            else
                Debug.LogWarning("HealthSlider não foi encontrado na cena!");
        }

        if (dashSlider == null)
        {
            GameObject dashObj = GameObject.Find("DashSlider");
            if (dashObj != null)
                dashSlider = dashObj.GetComponent<Slider>();
            else
                Debug.LogWarning("DashSlider não foi encontrado na cena!");
        }

        if (chargeSlider == null)
        {
            GameObject chargeObj = GameObject.Find("ChargeSlider");
            if (chargeObj != null)
                chargeSlider = chargeObj.GetComponent<Slider>();
            else
                Debug.LogWarning("ChargeSlider não foi encontrado na cena!");
        }

        if (heatSlider == null)
        {
            GameObject heatObj = GameObject.Find("HeatSlider");
            if (heatObj != null)
                heatSlider = heatObj.GetComponent<Slider>();
            else
                Debug.LogWarning("HeatSlider não foi encontrado na cena!");
        }

        // Busca o contador de moedas se não estiver atribuído via inspetor
        if (coinText == null)
        {
            GameObject coinObj = GameObject.Find("CoinText");
            if (coinObj != null)
                coinText = coinObj.GetComponent<TMP_Text>();  // usa TMP_Text
            else
                Debug.LogWarning("CoinText não foi encontrado na cena!");
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
                Debug.LogWarning("PlayerController não encontrado no prefab do Player!");
        }
    }

    void Start()
    {
        // Configura o valor máximo do slider de vida
        if (playerController != null && healthSlider != null)
        {
            Health h = playerController.GetComponent<Health>();
            if (h != null)
                healthSlider.maxValue = h.MaxHealth;
        }
    }

    void Update()
    {
        if (playerController != null)
        {
            if (healthSlider != null)
            {
                Health h = playerController.GetComponent<Health>();
                if (h != null)
                    healthSlider.value = h.CurrentHealth;
            }
            if (dashSlider != null)
                dashSlider.value = playerController.DashProgress;
            if (chargeSlider != null)
                chargeSlider.value = playerController.ChargeProgress;
            if (heatSlider != null)
                heatSlider.value = playerController.HeatProgress;
        }

        // Atualiza o contador de moedas usando TextMesh Pro
        if (coinText != null && GameManager.Instance != null)
        {
            coinText.text = "N₢$" + GameManager.Instance.CoinCount;
        }
    }
}
