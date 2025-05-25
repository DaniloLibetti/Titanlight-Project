using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Garante que o jogador tem collider
public class PickUpSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemsPickedText;
    private int itemsCount = 0;

    private void Start()
    {
        UpdateUI();
        Debug.Log("Sistema de coleta inicializado!");
    }

    public void AddItem(ItemSO itemSO, int quantity = 1)
    {
        itemsCount += quantity;
        Debug.Log($"Item coletado: {itemSO.name} (Total: {itemsCount})");
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (itemsPickedText != null)
        {
            itemsPickedText.text = "Espólios: " + itemsCount;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI não atribuído!");
        }
    }
}