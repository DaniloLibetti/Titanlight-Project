using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunSummary : MonoBehaviour
{
    [Header("Configurações de Run")]
    public float highMultiplier = 100f; // multiplicador de recompensa alta  
    public float neutralMultiplier = 80f; // multiplicador neutro  
    public float lowMultiplier = 60f; // multiplicador de recompensa baixa  

    [Header("Componentes da UI")]
    public Button optionHighButton; // botão da oferta alta  
    public Button optionNeutralButton; // botão da oferta neutra  
    public Button optionLowButton; // botão da oferta baixa  
    public TextMeshProUGUI optionHighText; // texto da oferta alta  
    public TextMeshProUGUI optionNeutralText; // texto da oferta neutra  
    public TextMeshProUGUI optionLowText; // texto da oferta baixa  
    public GameObject summaryCanvas; // painel de resumo  

    private int highOffer;
    private int neutralOffer;
    private int lowOffer;
    private int highReputationChange; // mudança de reputação (oferta alta)  
    private int lowReputationChange; // mudança de reputação (oferta baixa)  

    void Awake()
    {
        summaryCanvas?.SetActive(false); // inicia com o painel desativado  
        optionHighButton.onClick.AddListener(SelectHigh); // vincula botão alta  
        optionNeutralButton.onClick.AddListener(SelectNeutral); // vincula botão neutro  
        optionLowButton.onClick.AddListener(SelectLow); // vincula botão baixa  
    }

    public void ShowSummary()
    {
        CalculateProposals(); // calcula ofertas  
        SetUI(); // atualiza textos  
        summaryCanvas?.SetActive(true); // mostra o painel  
    }

    private void CalculateProposals()
    {
        int itemCount = GameManager.Instance.ScriptableObjectCount; // pega itens coletados  
        highOffer = Mathf.RoundToInt(itemCount * highMultiplier); // calcula oferta alta  
        neutralOffer = Mathf.RoundToInt(itemCount * neutralMultiplier); // oferta neutra  
        lowOffer = Mathf.RoundToInt(itemCount * lowMultiplier); // oferta baixa  
        highReputationChange = -Mathf.RoundToInt((highOffer / 5f) * 2f); // penaliza reputação (alta)  
        lowReputationChange = Mathf.RoundToInt((lowOffer / 3f) * 2f); // aumenta reputação (baixa)  
    }

    private void SetUI()
    {
        optionHighText.text = $"Oferta: {highOffer}\nReputação: {highReputationChange:+#;-#;0}";
        optionNeutralText.text = $"Oferta: {neutralOffer}\nReputação: +0"; // neutro não altera  
        optionLowText.text = $"Oferta: {lowOffer}\nReputação: {lowReputationChange:+#;-#;0}";
    }

    private void SelectHigh()
    {
        PlayerData.AddMoney(highOffer); // adiciona dinheiro (alta)  
        PlayerData.ChangeReputation(highReputationChange); // aplica mudança reputação  
        EndRun(); // fecha o painel  
    }

    private void SelectNeutral()
    {
        PlayerData.AddMoney(neutralOffer); // adiciona dinheiro neutro  
        EndRun();
    }

    private void SelectLow()
    {
        PlayerData.AddMoney(lowOffer); // adiciona dinheiro (baixa)  
        PlayerData.ChangeReputation(lowReputationChange); // aplica mudança reputação  
        EndRun();
    }

    public void EndRun()
    {
        summaryCanvas?.SetActive(false); // esconde painel  
        GameManager.Instance.CompleteAuction(); // finaliza lógica do leilão  
    }
}