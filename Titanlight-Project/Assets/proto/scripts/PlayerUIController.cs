using UnityEngine;
using UnityEngine.UI;
using Player.StateMachine;

/// <summary>
/// Controla os sliders de UI automaticamente, encontrando os objetos na cena:
/// - Vida do jogador
/// - Energia para dash (0 a 100)
/// - Heat da metralhadora
/// </summary>
public class PlayerUIController : MonoBehaviour
{
    private Slider healthSlider;
    private Slider dashEnergySlider;
    private Slider heatSlider;

    private PlayerStateMachine player;
    private Health playerHealth;

    private bool lastCanDash;
    private float dashStartTime;

    void Awake()
    {
        // Encontrar sliders na cena pelo nome
        var sliders = FindObjectsOfType<Slider>();
        foreach (var s in sliders)
        {
            string lower = s.gameObject.name.ToLower();
            if (lower.Contains("health"))
                healthSlider = s;
            else if (lower.Contains("dash"))
                dashEnergySlider = s;
            else if (lower.Contains("heat"))
                heatSlider = s;
        }

        if (healthSlider == null)
            Debug.LogWarning("[UI] Health slider não encontrado.");
        if (dashEnergySlider == null)
            Debug.LogWarning("[UI] Dash energy slider não encontrado.");
        if (heatSlider == null)
            Debug.LogWarning("[UI] Heat slider não encontrado.");

        player = PlayerStateMachine.Instance;
        if (player != null)
            playerHealth = player.GetComponent<Health>();
    }

    void Start()
    {
        InitializeUI();
    }

    void Update()
    {
        // Caso o player seja instanciado depois
        if (player == null)
        {
            player = PlayerStateMachine.Instance;
            if (player != null)
                playerHealth = player.GetComponent<Health>();
            else
                return;
        }

        // Atualiza vida
        if (healthSlider != null && playerHealth != null)
            healthSlider.value = playerHealth.CurrentHealth;

        // Atualiza dash energy (0 a 100)
        if (dashEnergySlider != null)
        {
            float dashCd = player.config.dashCooldown;
            dashEnergySlider.maxValue = 100f;

            // Detecta início do consumo
            if (lastCanDash && !player.CanDash)
                dashStartTime = Time.time;
            lastCanDash = player.CanDash;

            float percent = player.CanDash
                ? 1f
                : Mathf.Clamp((Time.time - dashStartTime) / dashCd, 0f, 1f);

            dashEnergySlider.value = percent * 100f;
        }

        // Atualiza heat
        if (heatSlider != null)
        {
            heatSlider.maxValue = player.config.heatMax;
            heatSlider.value = player.currentHeat;
        }
    }

    /// <summary>
    /// Inicializa valores máximos e estado inicial dos sliders.
    /// </summary>
    private void InitializeUI()
    {
        if (player == null)
            player = PlayerStateMachine.Instance;
        if (player == null)
            return;

        playerHealth = player.GetComponent<Health>();

        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.CurrentHealth;
        }

        if (dashEnergySlider != null)
        {
            dashEnergySlider.maxValue = 100f;
            dashEnergySlider.value = player.CanDash ? 100f : 0f;
            lastCanDash = player.CanDash;
            dashStartTime = Time.time - (player.CanDash ? player.config.dashCooldown : 0f);
        }

        if (heatSlider != null)
        {
            heatSlider.maxValue = player.config.heatMax;
            heatSlider.value = player.currentHeat;
        }
    }
}
