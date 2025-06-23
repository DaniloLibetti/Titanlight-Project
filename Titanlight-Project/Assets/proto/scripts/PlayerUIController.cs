using UnityEngine;
using UnityEngine.UI;
using Player.StateMachine; // Adicionando a referência necessária

public class PlayerUIController : MonoBehaviour
{
    private Slider healthSlider;
    private Health playerHealth;

    [Header("Player Settings")]
    [Range(1, 2)] public int playerIndex = 1;

    void Awake()
    {
        // Encontra o slider de saúde na cena
        var sliders = FindObjectsOfType<Slider>();
        foreach (var s in sliders)
        {
            if (s.gameObject.name.ToLower().Contains("health"))
            {
                healthSlider = s;
                break;
            }
        }

        if (healthSlider == null)
            Debug.LogWarning("[UI] Health slider não encontrado.");
    }

    void Start()
    {
        InitializeUI();
    }

    void Update()
    {
        // Atualiza o valor do slider de vida
        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.value = playerHealth.CurrentHealth;
        }
        else
        {
            // Tenta re-inicializar se as referências estiverem perdidas
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        // Verifica se o PlayerManager está disponível
        if (PlayerManager.Instance == null)
        {
            Debug.LogWarning("PlayerManager não encontrado");
            return;
        }

        // Obtém a referência ao jogador correto
        GameObject playerObject = playerIndex == 1 ?
            PlayerManager.Instance.Player1 :
            PlayerManager.Instance.Player2;

        if (playerObject == null)
        {
            Debug.LogWarning($"Jogador {playerIndex} não encontrado");
            return;
        }

        // Obtém o componente Health diretamente
        playerHealth = playerObject.GetComponent<Health>();

        if (playerHealth == null)
        {
            Debug.LogWarning($"Componente Health não encontrado no jogador {playerIndex}");
            return;
        }

        // Configura o slider de vida
        if (healthSlider != null)
        {
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.CurrentHealth;
        }
    }
}