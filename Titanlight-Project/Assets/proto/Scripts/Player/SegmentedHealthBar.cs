using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SegmentedHealthBar : MonoBehaviour
{
    /*[Header("Player Settings")]
    [Tooltip("Player index (1 or 2)")]
    public int playerIndex = 1;

    [Header("UI References")]
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectMask2D mask;
    [SerializeField] private GameObject healthBarObject;

    private Health healthPlayer;
    private float maxRightMask;
    private float initialRightMask;
    private float barWidth;

    private void Start()
    {
        // Começa desativado até encontrar o jogador
        if (healthBarObject != null) healthBarObject.SetActive(false);

        // Configuração inicial
        SetupHealthBar();

        // Tenta encontrar o jogador imediatamente
        FindPlayer();
    }

    private void SetupHealthBar()
    {
        if (barRect != null && mask != null)
        {
            barWidth = barRect.rect.width;
            initialRightMask = mask.padding.z;
            maxRightMask = barWidth - mask.padding.x - initialRightMask;
        }
        else
        {
            Debug.LogError("UI references missing!", this);
        }
    }

    private void FindPlayer()
    {
        // Se for Player 2 em singleplayer, desativa a barra
        if (playerIndex == 2 && !PlayerManager.Instance.isMultiplayer)
        {
            HideBar();
            return;
        }

        // Tenta encontrar o Health do jogador
        Health targetHealth = FindPlayerHealth();

        if (targetHealth != null)
        {
            ConnectToPlayer(targetHealth);
        }
        else
        {
            // Tenta novamente após um delay
            StartCoroutine(RetryFindPlayer());
        }
    }

    private Health FindPlayerHealth()
    {
        // Procura o PlayerManager
        if (PlayerManager.Instance != null)
        {
            if (playerIndex == 1 && PlayerManager.Instance.Player1 != null)
                return PlayerManager.Instance.Player1.GetComponent<Health>();

            if (playerIndex == 2 && PlayerManager.Instance.Player2 != null)
                return PlayerManager.Instance.Player2.GetComponent<Health>();
        }

        // Método alternativo: Busca por nome
        GameObject playerObj = GameObject.Find($"Player {playerIndex}");
        if (playerObj != null) return playerObj.GetComponent<Health>();

        // Último recurso: Busca por tag
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.name == $"Player {playerIndex}")
            {
                return player.GetComponent<Health>();
            }
        }

        return null;
    }

    private void ConnectToPlayer(Health playerHealth)
    {
        healthPlayer = playerHealth;

        // Conecta aos eventos de vida
        healthPlayer.onDamageTaken.AddListener(UpdateHealthBar);
        healthPlayer.onDeath.AddListener(OnPlayerDeath);

        // Atualiza a barra imediatamente
        UpdateHealthBar(0);

        // Mostra a HUD
        ShowBar();

        Debug.Log($"[HealthBar] Connected to Player {playerIndex}");
    }

    private IEnumerator RetryFindPlayer()
    {
        yield return new WaitForSeconds(0.5f);
        FindPlayer();
    }

    private void OnDestroy()
    {
        if (healthPlayer != null)
        {
            healthPlayer.onDamageTaken.RemoveListener(UpdateHealthBar);
            healthPlayer.onDeath.RemoveListener(OnPlayerDeath);
        }
    }

    public void UpdateHealthBar(float damageTaken)
    {
        if (healthPlayer == null || mask == null) return;

        float healthPercent = Mathf.Clamp01(healthPlayer.CurrentHealth / healthPlayer.MaxHealth);
        float targetRightMask = maxRightMask * (1 - healthPercent) + initialRightMask;

        var padding = mask.padding;
        padding.z = targetRightMask;
        mask.padding = padding;
    }

    private void OnPlayerDeath()
    {
        UpdateHealthBar(0);
        Debug.Log($"[HealthBar] Player {playerIndex} died");
    }

    private void ShowBar()
    {
        if (healthBarObject != null)
        {
            healthBarObject.SetActive(true);
            Debug.Log($"[HealthBar] Player {playerIndex} bar shown");
        }
    }

    private void HideBar()
    {
        if (healthBarObject != null)
        {
            healthBarObject.SetActive(false);
            Debug.Log($"[HealthBar] Player {playerIndex} bar hidden");
        }
    }*/
}