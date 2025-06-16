using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunSummary : MonoBehaviour
{
    [Header("Configurações de Run")]
    public float highMultiplier = 100f;   // multiplicador de recompensa alta  
    public float neutralMultiplier = 80f; // multiplicador neutro  
    public float lowMultiplier = 60f;     // multiplicador de recompensa baixa  

    [Header("Componentes da UI")]
    public Button optionHighButton;      // botão da oferta alta  
    public Button optionNeutralButton;   // botão da oferta neutra  
    public Button optionLowButton;       // botão da oferta baixa  
    public TextMeshProUGUI optionHighText;    // texto da oferta alta  
    public TextMeshProUGUI optionNeutralText; // texto da oferta neutra  
    public TextMeshProUGUI optionLowText;     // texto da oferta baixa  
    public GameObject summaryCanvas;          // painel de resumo  

    private int highOffer;
    private int neutralOffer;
    private int lowOffer;
    private int highReputationChange; // normalmente negativo
    private int lowReputationChange;  // normalmente positivo

    void Awake()
    {
        if (summaryCanvas != null)
            summaryCanvas.SetActive(false);

        if (optionHighButton != null) optionHighButton.onClick.AddListener(SelectHigh);
        if (optionNeutralButton != null) optionNeutralButton.onClick.AddListener(SelectNeutral);
        if (optionLowButton != null) optionLowButton.onClick.AddListener(SelectLow);
    }

    public void ShowSummary()
    {
        CalculateProposals();
        SetUI();
        if (summaryCanvas != null)
            summaryCanvas.SetActive(true);
    }

    private void CalculateProposals()
    {
        int itemCount = 0;
        if (GameManager.Instance != null)
            itemCount = GameManager.Instance.ScriptableObjectCount;
        highOffer = Mathf.RoundToInt(itemCount * highMultiplier);
        neutralOffer = Mathf.RoundToInt(itemCount * neutralMultiplier);
        lowOffer = Mathf.RoundToInt(itemCount * lowMultiplier);

        // Alta oferta: penaliza reputação (valor negativo)
        highReputationChange = -Mathf.RoundToInt((highOffer / 5f) * 2f);
        // Baixa oferta: aumenta reputação
        lowReputationChange = Mathf.RoundToInt((lowOffer / 3f) * 2f);
    }

    private void SetUI()
    {
        if (optionHighText != null)
            optionHighText.text = $"Oferta: {highOffer}\nReputação: {highReputationChange:+#;-#;0}";
        if (optionNeutralText != null)
            optionNeutralText.text = $"Oferta: {neutralOffer}\nReputação: +0";
        if (optionLowText != null)
            optionLowText.text = $"Oferta: {lowOffer}\nReputação: {lowReputationChange:+#;-#;0}";
    }

    private void SelectHigh()
    {
        PlayerRuntimeData.AddMoney(highOffer);
        PlayerRuntimeData.AddReputation(highReputationChange);
        EndRun();
    }

    private void SelectNeutral()
    {
        PlayerRuntimeData.AddMoney(neutralOffer);
        // reputação inalterada
        EndRun();
    }

    private void SelectLow()
    {
        PlayerRuntimeData.AddMoney(lowOffer);
        PlayerRuntimeData.AddReputation(lowReputationChange);
        EndRun();
    }

    private void EndRun()
    {
        if (summaryCanvas != null)
            summaryCanvas.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.CompleteAuction();
    }
}
