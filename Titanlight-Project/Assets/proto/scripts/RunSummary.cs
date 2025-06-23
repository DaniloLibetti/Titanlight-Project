using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class RunSummary : MonoBehaviour
{
    [Header("Configurações de Run")]
    public float highMultiplier = 100f;   
    public float neutralMultiplier = 80f;
    public float lowMultiplier = 60f;   

    [Header("Limites de reputação")]
    public int minReputationChange = 5;
    public int maxReputationChange = 20;

    [Header("Componentes da UI")]
    public Button optionHighButton;     
    public Button optionNeutralButton;   
    public Button optionLowButton;      

    [Header("Textos de valor e reputação")]
    public TextMeshProUGUI optionHighValueText;      
    public TextMeshProUGUI optionHighReputationText;

    public TextMeshProUGUI optionNeutralValueText;     
    public TextMeshProUGUI optionNeutralReputationText;

    public TextMeshProUGUI optionLowValueText;     
    public TextMeshProUGUI optionLowReputationText; 

    [Header("Panel de resumo")]
    public GameObject summaryCanvas;        

    [Header("Referência direta ao GameManager")]
    public GameManager gameManagerRef; 

    private int highOffer;
    private int neutralOffer;
    private int lowOffer;
    private int highReputationChange;
    private int lowReputationChange;

    void Awake()
    {
        if (gameManagerRef == null)
        {
            gameManagerRef = GameManager.Instance;
        }
        Debug.Log("[RunSummary] Awake: gameManagerRef = " + gameManagerRef);

        if (summaryCanvas != null)
            summaryCanvas.SetActive(false);

        if (optionHighButton != null) optionHighButton.onClick.AddListener(SelectHigh);
        if (optionNeutralButton != null) optionNeutralButton.onClick.AddListener(SelectNeutral);
        if (optionLowButton != null) optionLowButton.onClick.AddListener(SelectLow);
    }

    
    public void ShowSummary()
    {
        if (gameManagerRef == null)
        {
            Debug.LogError("[RunSummary] ShowSummary mas gameManagerRef é null!");
            return;
        }
        int itemCount = gameManagerRef.ScriptableObjectCount;
        Debug.Log("[RunSummary] ShowSummary: itemCount = " + itemCount);

        CalculateProposals(itemCount);
        SetUI();

        if (summaryCanvas != null)
        {
            summaryCanvas.SetActive(true);
            Debug.Log("[RunSummary] summaryCanvas ativado");
        }
        else
        {
            Debug.LogWarning("[RunSummary] summaryCanvas não atribuído!");
        }
    }

    private void CalculateProposals(int itemCount)
    {
        highOffer = Mathf.RoundToInt(itemCount * highMultiplier);
        neutralOffer = Mathf.RoundToInt(itemCount * neutralMultiplier);
        lowOffer = Mathf.RoundToInt(itemCount * lowMultiplier);

        highOffer = Mathf.Clamp(highOffer, 0, 999_999_999);
        neutralOffer = Mathf.Clamp(neutralOffer, 0, 999_999_999);
        lowOffer = Mathf.Clamp(lowOffer, 0, 999_999_999);

        float reputationFactor = Mathf.Clamp01(itemCount / 100f);
        int repChange = Mathf.RoundToInt(minReputationChange +
                            (maxReputationChange - minReputationChange) * reputationFactor);

        highReputationChange = -repChange; 
        lowReputationChange = repChange;  

        Debug.Log($"[RunSummary] Calculated offers: highOffer={highOffer}, neutralOffer={neutralOffer}, lowOffer={lowOffer}; rep: high={highReputationChange}, low={lowReputationChange}");
    }

    private void SetUI()
    {
        if (optionHighValueText != null)
            optionHighValueText.text = FormatNumber(highOffer);
        else
            Debug.LogWarning("[RunSummary] optionHighValueText não atribuído!");

        if (optionHighReputationText != null)
        {
            optionHighReputationText.text = highReputationChange.ToString();
            optionHighReputationText.color = Color.red;
        }
        else
            Debug.LogWarning("[RunSummary] optionHighReputationText não atribuído!");

        if (optionNeutralValueText != null)
            optionNeutralValueText.text = FormatNumber(neutralOffer);
        else
            Debug.LogWarning("[RunSummary] optionNeutralValueText não atribuído!");

        if (optionNeutralReputationText != null)
        {
            optionNeutralReputationText.text = "0";
            optionNeutralReputationText.color = Color.white;
        }
        else
            Debug.LogWarning("[RunSummary] optionNeutralReputationText não atribuído!");

        if (optionLowValueText != null)
            optionLowValueText.text = FormatNumber(lowOffer);
        else
            Debug.LogWarning("[RunSummary] optionLowValueText não atribuído!");

        if (optionLowReputationText != null)
        {
            optionLowReputationText.text = $"+{lowReputationChange}";
            optionLowReputationText.color = new Color(0.2f, 0.8f, 0.2f);
        }
        else
            Debug.LogWarning("[RunSummary] optionLowReputationText não atribuído!");

        if (optionHighButton != null)
        {
            var txt = optionHighButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "Mercado Negro";
        }
        if (optionNeutralButton != null)
        {
            var txt = optionNeutralButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "Neutra";
        }
        if (optionLowButton != null)
        {
            var txt = optionLowButton.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = "Coletores";
        }
    }

    private string FormatNumber(int value)
    {
        string s = value.ToString("N0");
        s = s.Replace(",", ".");
        return s;
    }

    private void SelectHigh()
    {
        Debug.Log("[RunSummary] SelectHigh chamado");
        PlayerRuntimeData.AddMoney(highOffer);
        PlayerRuntimeData.AddReputation(highReputationChange);
        EndRunSummary();
    }

    private void SelectNeutral()
    {
        Debug.Log("[RunSummary] SelectNeutral chamado");
        PlayerRuntimeData.AddMoney(neutralOffer);
        EndRunSummary();
    }

    private void SelectLow()
    {
        Debug.Log("[RunSummary] SelectLow chamado");
        PlayerRuntimeData.AddMoney(lowOffer);
        PlayerRuntimeData.AddReputation(lowReputationChange);
        EndRunSummary();
    }

    private void EndRunSummary()
    {
        if (summaryCanvas != null)
            summaryCanvas.SetActive(false);

        if (gameManagerRef != null)
            gameManagerRef.CompleteAuction();
        else
            Debug.LogWarning("[RunSummary] gameManagerRef é null em EndRunSummary");
    }
}
