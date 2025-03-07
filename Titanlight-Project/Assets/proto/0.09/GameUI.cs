using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI interactionText;

    private void Start()
    {
        GameManager.Instance.OnRoomChanged += UpdateRoomName;
        UpdateRoomName(GameManager.Instance.GetCurrentRoomCoord());
        interactionText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnRoomChanged -= UpdateRoomName;
    }

    public void UpdateRoomName(Vector2Int coord)
    {
        // Converte X para letra e Y para número (base 1)
        char xChar = (char)('A' + coord.x);
        int yNumber = coord.y + 1;
        roomNameText.text = $"{xChar}{yNumber}";
    }

    public void ShowInteractionText(bool show)
    {
        interactionText.gameObject.SetActive(show);
    }
}