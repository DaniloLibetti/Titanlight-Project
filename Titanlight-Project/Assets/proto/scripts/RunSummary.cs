using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunSummary : MonoBehaviour
{
    [Header("Configurações")]
    public float highMultiplier = 100f;
    public float neutralMultiplier = 20f;
    public float lowMultiplier = 60f;

    [Header("UI")]
    public Button optionHighButton;
    public Button optionNeutralButton;
    public Button optionLowButton;
    public TextMeshProUGUI optionHighText;
    public TextMeshProUGUI optionNeutralText;
    public TextMeshProUGUI optionLowText;
    public GameObject summaryCanvas;

    private int highOffer;
    private int neutralOffer;
    private int lowOffer;
    private int highReputationChange;
    private int lowReputationChange;

    void Awake()
    {
        summaryCanvas?.SetActive(false);
        optionHighButton.onClick.AddListener(SelectHigh);
        optionNeutralButton.onClick.AddListener(SelectNeutral);
        optionLowButton.onClick.AddListener(SelectLow);
    }

    public void ShowSummary()
    {
        CalculateProposals();
        SetUI();
        summaryCanvas?.SetActive(true);
    }

    private void CalculateProposals()
    {
        int itemCount = GameManager.Instance.ScriptableObjectCount;

        // Cálculos corrigidos para precisão
        highOffer = Mathf.RoundToInt(itemCount * highMultiplier);
        neutralOffer = Mathf.RoundToInt(itemCount * neutralMultiplier);
        lowOffer = Mathf.RoundToInt(itemCount * lowMultiplier);

        // Valores de reputação ajustados para inteiros exatos
        highReputationChange = -Mathf.RoundToInt(highOffer * 0.4f); // -40% do valor
        lowReputationChange = Mathf.RoundToInt(lowOffer * 0.3f); // +30% do valor
    }

    private void SetUI()
    {
        optionHighText.text = $"Oferta: {highOffer}\nReputação: {highReputationChange}";
        optionNeutralText.text = $"Oferta: {neutralOffer}\nReputação: 0";
        optionLowText.text = $"Oferta: {lowOffer}\nReputação: +{lowReputationChange}";
    }

    private void SelectHigh()
    {
        ApplyChanges(highOffer, highReputationChange);
    }

    private void SelectNeutral()
    {
        ApplyChanges(neutralOffer, 0);
    }

    private void SelectLow()
    {
        ApplyChanges(lowOffer, lowReputationChange);
    }

    private void ApplyChanges(int money, int reputation)
    {
        PlayerRuntimeData.AddMoney(money);
        PlayerRuntimeData.ChangeReputation(reputation);
        EndRun();
    }

    public void EndRun()
    {
        summaryCanvas?.SetActive(false);
        GameManager.Instance.CompleteAuction();
    }
}