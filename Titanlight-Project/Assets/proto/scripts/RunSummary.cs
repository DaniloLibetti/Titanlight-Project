using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [Tooltip("Painel que contém os botões e textos de oferta")]
    public GameObject summaryCanvas;    // mantenha desativado no Inspector

    private int coinCount;
    private int highOffer;
    private int neutralOffer;
    private int lowOffer;
    private int highReputationChange;
    private int lowReputationChange;

    void Awake()
    {
        // Garante que o painel comece fechado
        if (summaryCanvas != null)
            summaryCanvas.SetActive(false);

        // Liga os botões às funções
        optionHighButton.onClick.AddListener(SelectHigh);
        optionNeutralButton.onClick.AddListener(SelectNeutral);
        optionLowButton.onClick.AddListener(SelectLow);
    }

    /// <summary>
    /// Exibe o painel de resumo e atualiza os valores.
    /// </summary>
    public void ShowSummary()
    {
        CalculateProposals();
        SetUI();

        if (summaryCanvas != null)
            summaryCanvas.SetActive(true);
    }

    private void CalculateProposals()
    {
        // Pega quantas moedas foram coletadas nesta run
        coinCount = GameManager.Instance.CoinCount;

        // Calcula cada oferta
        highOffer = Mathf.RoundToInt(coinCount * highMultiplier);
        highReputationChange = -Mathf.RoundToInt((highOffer / 5f) * 2f);

        neutralOffer = Mathf.RoundToInt(coinCount * neutralMultiplier);

        lowOffer = Mathf.RoundToInt(coinCount * lowMultiplier);
        lowReputationChange = Mathf.RoundToInt((lowOffer / 3f) * 2f);
    }

    private void SetUI()
    {
        // Formata com sinal + ou –
        optionHighText.text = $"Oferta: {highOffer}\nReputação: {highReputationChange:+#;-#;0}";
        optionNeutralText.text = $"Oferta: {neutralOffer}\nReputação: +0";
        optionLowText.text = $"Oferta: {lowOffer}\nReputação: {lowReputationChange:+#;-#;0}";
    }

    private void SelectHigh()
    {
        PlayerData.AddMoney(highOffer);
        PlayerData.ChangeReputation(highReputationChange);
        EndRun();
    }

    private void SelectNeutral()
    {
        PlayerData.AddMoney(neutralOffer);
        EndRun();
    }

    private void SelectLow()
    {
        PlayerData.AddMoney(lowOffer);
        PlayerData.ChangeReputation(lowReputationChange);
        EndRun();
    }

    public void EndRun()
    {
        if (summaryCanvas != null)
            summaryCanvas.SetActive(false);

        // chama o CompleteAuction que agora reseta para o MoonBox
        GameManager.Instance.CompleteAuction();
    }
}
