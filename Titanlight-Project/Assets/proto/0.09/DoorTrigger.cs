using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Vector2Int roomCoord;
    public DoorDirection direction;
    public KeyCode interactKey = KeyCode.E;
    public float hackDistance = 1f;

    public DoorState State { get; private set; }
    private bool _isPlayerInTrigger = false;

    private void Start()
    {
        InitializeDoorState();
    }

    void InitializeDoorState()
    {
        if (!ValidateGameManager() || !ValidateCoordinates()) return;

        Room room = GameManager.Instance.GetRoom(roomCoord);
        if (room != null)
        {
            State = room.GetDoorState(direction);
        }
        else
        {
            Debug.LogError($"Sala {roomCoord} não encontrada!");
            gameObject.SetActive(false);
        }
    }

    bool ValidateGameManager()
    {
        if (GameManager.Instance != null) return true;
        Debug.LogError("GameManager não encontrado!");
        return false;
    }

    bool ValidateCoordinates()
    {
        if (roomCoord.x >= 0 && roomCoord.x < GameManager.Instance.gridSize.x &&
            roomCoord.y >= 0 && roomCoord.y < GameManager.Instance.gridSize.y)
            return true;

        Debug.LogError($"Coordenada inválida: {roomCoord}");
        gameObject.SetActive(false);
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _isPlayerInTrigger = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = false;
            SafeUpdateUI(false);
        }
    }

    private void Update()
    {
        if (_isPlayerInTrigger && State != null)
        {
            UpdateInteractionUI();
            HandleInput();
        }
    }

    void UpdateInteractionUI()
    {
        if (GameUI.Instance == null) return;

        string text = State.isLocked ? "Porta Trancada (H para hackear)" :
                      !State.isOpen ? "Porta Quebrada" :
                      "Pressione E para entrar";

        SafeUpdateUI(true, text);
    }

    void SafeUpdateUI(bool show, string text = "")
    {
        if (GameUI.Instance == null) return;

        // Utiliza o método ToggleInteractionText conforme corrigido
        GameUI.Instance.ToggleInteractionText(show);
        if (!string.IsNullOrEmpty(text))
            GameUI.Instance.SetInteractionText(text);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(interactKey))
            TryInteractWithDoor();
    }

    void TryInteractWithDoor()
    {
        if (State.isOpen && !State.isLocked)
            GameManager.Instance.TryMoveToRoom(direction.ToVector(), Player.Instance.transform);
    }

    public void UnlockDoor()
    {
        if (State == null) return;
        State.isLocked = false;
        State.wasHacked = true;
        State.isOpen = true;
    }
}
