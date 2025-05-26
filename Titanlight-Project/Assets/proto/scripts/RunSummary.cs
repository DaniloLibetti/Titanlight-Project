using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunSummary : MonoBehaviour
{
    [Header("Configurações de Run")]
    [Tooltip("Mercado Negro: melhor oferta, reputação é negativa (%)")]
    public float marketMultiplier = 100f;
    [Tooltip("Varejo: oferta intermediária, sem mudança de reputação")]
    public float retailMultiplier = 80f;
    [Tooltip("TitanLight: pior oferta, reputação é positiva (%)")]
    public float titanMultiplier = 60f;

    [Header("Reputação baseada na oferta")]
    [Tooltip("Percentual de reputação negativa para Mercado Negro (ex: 0.12 = 12% de marketOffer)")]
    [Range(0f, 1f)]
    public float marketReputationPercent = 0.12f;
    [Tooltip("Percentual de reputação positiva para TitanLight (ex: 0.20 = 20% de titanOffer)")]
    [Range(0f, 1f)]
    public float titanReputationPercent = 0.20f;

    [Header("Componentes da UI")]
    // Mercado Negro
    public Button marketButton;           
    // Varejo
    public Button retailButton;     
    // TitanLight
    public Button titanButton;

    public TextMeshProUGUI marketOfferText;
    public TextMeshProUGUI marketReputationText;

    public TextMeshProUGUI retailOfferText;
    public TextMeshProUGUI retailReputationText;

    public TextMeshProUGUI titanOfferText;
    public TextMeshProUGUI titanReputationText;

    public GameObject summaryCanvas;

    private int marketOffer, retailOffer, titanOffer;
    private int marketReputationChange, titanReputationChange;

    void Awake()
    {
        summaryCanvas?.SetActive(false);

        marketButton.onClick.AddListener(SelectMarket);
        retailButton.onClick.AddListener(SelectRetail);
        titanButton.onClick.AddListener(SelectTitan);
    }

    public void ShowSummary()
    {
        CalculateOffersAndReputation();
        UpdateUI();
        summaryCanvas?.SetActive(true);
    }

    private void CalculateOffersAndReputation()
    {
        int itemCount = GameManager.Instance.ScriptableObjectCount;

        marketOffer = Mathf.RoundToInt(itemCount * marketMultiplier);
        retailOffer = Mathf.RoundToInt(itemCount * retailMultiplier);
        titanOffer = Mathf.RoundToInt(itemCount * titanMultiplier);

        marketReputationChange = -Mathf.RoundToInt(marketOffer * marketReputationPercent);
        titanReputationChange = Mathf.RoundToInt(titanOffer * titanReputationPercent);
    }

    private void UpdateUI()
    {
        // Mercado Negro
        marketOfferText.text = marketOffer.ToString();
        marketReputationText.text = FormatReputation(marketReputationChange);

        // Varejo
        retailOfferText.text = retailOffer.ToString();
        retailReputationText.text = FormatReputation(0);

        // TitanLight
        titanOfferText.text = titanOffer.ToString();
        titanReputationText.text = FormatReputation(titanReputationChange);
    }

    private string FormatReputation(int rep)
    {
        string sign = rep > 0 ? "+" : "";
        return $"{sign}{rep} of reputation";
    }

    private void SelectMarket()
    {
        PlayerData.AddMoney(marketOffer);
        PlayerData.ChangeReputation(marketReputationChange);
        EndRun();
    }

    private void SelectRetail()
    {
        PlayerData.AddMoney(retailOffer);
        EndRun();
    }

    private void SelectTitan()
    {
        PlayerData.AddMoney(titanOffer);
        PlayerData.ChangeReputation(titanReputationChange);
        EndRun();
    }

    public void EndRun()
    {
        summaryCanvas?.SetActive(false);
        GameManager.Instance.CompleteAuction();
    }
}
