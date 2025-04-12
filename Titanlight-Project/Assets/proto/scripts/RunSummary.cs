using UnityEngine;
using UnityEngine.UI;
using TMPro;
// Se for reiniciar a cena ao final, descomente a linha abaixo
// using UnityEngine.SceneManagement;

public class RunSummary : MonoBehaviour
{
    [Header("Configurações de Run")]
    [Tooltip("Multiplicador para a oferta de alta recompensa")]
    public float highMultiplier = 100f;
    [Tooltip("Multiplicador para a oferta neutra")]
    public float neutralMultiplier = 80f;
    [Tooltip("Multiplicador para a oferta de baixa recompensa")]
    public float lowMultiplier = 60f;

    [Header("Componentes da UI")]
    public Button optionHighButton;
    public Button optionNeutralButton;
    public Button optionLowButton;

    public TextMeshProUGUI optionHighText;
    public TextMeshProUGUI optionNeutralText;
    public TextMeshProUGUI optionLowText;

    [Tooltip("Canvas do leilão (resumo da run)")]
    public GameObject summaryCanvas;

    private int coinCount;
    private int highOffer;
    private int neutralOffer;
    private int lowOffer;
    private int highReputationChange;
    private int lowReputationChange;

    void Awake()
    {
        optionHighButton.onClick.AddListener(SelectHigh);
        optionNeutralButton.onClick.AddListener(SelectNeutral);
        optionLowButton.onClick.AddListener(SelectLow);
    }

    void OnEnable()
    {
        CalculateProposals();
        SetUI();
    }

    /// <summary>
    /// Exibe o canvas de resumo.
    /// </summary>
    public void ShowSummary()
    {
        summaryCanvas.SetActive(true);
    }

    /// <summary>
    /// Calcula as propostas com base nas moedas acumuladas no PlayerData.
    /// </summary>
    void CalculateProposals()
    {
        // Pega o total de moedas que o jogador acumulou
        coinCount = PlayerData.GetMoney();
        Debug.Log($"[RunSummary] coinCount obtido: {coinCount}");

        // Alta recompensa: moedas * 100
        highOffer = Mathf.RoundToInt(coinCount * highMultiplier);
        // Perda de reputação: (highOffer / 5) * 2
        highReputationChange = -Mathf.RoundToInt((highOffer / 5f) * 2f);

        // Baixa recompensa: moedas * 60
        lowOffer = Mathf.RoundToInt(coinCount * lowMultiplier);
        // Ganho de reputação: (lowOffer / 3) * 2
        lowReputationChange = Mathf.RoundToInt((lowOffer / 3f) * 2f);

        // Oferta neutra: moedas * 80, sem reputação
        neutralOffer = Mathf.RoundToInt(coinCount * neutralMultiplier);
    }

    /// <summary>
    /// Atualiza os textos dos botões com valores calculados.
    /// </summary>
    void SetUI()
    {
        optionHighText.text = $"Receber {highOffer} moedas ({highReputationChange} Rep)";
        optionNeutralText.text = $"Receber {neutralOffer} moedas (Neutro)";
        optionLowText.text = $"Receber {lowOffer} moedas (+{lowReputationChange} Rep)";
    }

    void SelectHigh()
    {
        PlayerData.AddMoney(highOffer);
        PlayerData.ChangeReputation(highReputationChange);
        EndRun();
    }

    void SelectNeutral()
    {
        PlayerData.AddMoney(neutralOffer);
        EndRun();
    }

    void SelectLow()
    {
        PlayerData.AddMoney(lowOffer);
        PlayerData.ChangeReputation(lowReputationChange);
        EndRun();
    }

    /// <summary>
    /// Finaliza a run: esconde o canvas e retorna ao estado inicial.
    /// </summary>
    public void EndRun()
    {
        if (summaryCanvas != null)
            summaryCanvas.SetActive(false);

        Debug.Log("Run encerrada. Proposta escolhida aplicada.");

        // Se quiser recarregar a cena inicial da run, descomente:
        // SceneManager.LoadScene("NomeDaCenaInicial");
    }
}
