using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI interactionText;

    public static GameUI Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoomChanged += UpdateRoomName;
            UpdateRoomName(GameManager.Instance.GetCurrentRoomCoord());
        }
        ToggleInteractionText(false);
    }

    public void UpdateRoomName(Vector2Int coord)
    {
        if (roomNameText == null) return;
        roomNameText.text = $"{(char)('A' + coord.x)}{coord.y + 1}";
    }

    public void ToggleInteractionText(bool show)
    {
        if (interactionText != null)
            interactionText.gameObject.SetActive(show);
    }

    public void SetInteractionText(string text)
    {
        if (interactionText != null)
            interactionText.text = text;
    }
}
