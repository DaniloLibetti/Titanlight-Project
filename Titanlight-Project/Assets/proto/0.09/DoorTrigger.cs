using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorDirection direction;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode hackKey = KeyCode.H;

    [Tooltip("Distância que o jogador se moverá ao usar esta porta (pode ser personalizada para cada direção).")]
    public float moveDistance = 3f;

    [Tooltip("Tempo a ser reduzido no timer ao hackear esta porta.")]
    public float hackTimeReduction = 10f;

    // Se não atribuído no Inspector, será procurado automaticamente
    public CountdownTimer timer;

    private DoorState _state;
    private bool _isPlayerInRange;

    private void Start()
    {
        // Tenta encontrar o timer caso não tenha sido atribuído manualmente
        if (timer == null)
        {
            timer = FindObjectOfType<CountdownTimer>();
            if (timer == null)
            {
                Debug.LogWarning("CountdownTimer não encontrado na cena!");
            }
        }

        // Obtém o estado da porta a partir da sala (componente pai)
        Room room = GetComponentInParent<Room>();
        if (room != null)
        {
            _state = room.GetDoorState(direction);
        }
        else
        {
            Debug.LogError("Room não encontrada no pai da porta!");
        }
    }

    private void Update()
    {
        if (!_isPlayerInRange) return;

        UpdateUI();
        HandleInput();
    }

    private void UpdateUI()
    {
        if (_state.isLocked)
        {
            GameUI.Instance.SetInteractionText("[H] Hackear Porta");
        }
        else if (!_state.isOpen)
        {
            GameUI.Instance.SetInteractionText("Porta Quebrada");
        }
        else
        {
            GameUI.Instance.SetInteractionText("[E] Entrar");
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(hackKey) && _state.isLocked)
        {
            UnlockDoor();
        }
        else if (Input.GetKeyDown(interactKey))
        {
            TryPassThroughDoor();
        }
    }

    private void TryPassThroughDoor()
    {
        if (_state.isOpen && !_state.isLocked)
        {
            // Chama o método do GameManager e passa a distância configurada para esta porta
            GameManager.Instance.TryMoveThroughDoor(direction, moveDistance);
        }
    }

    public void UnlockDoor()
    {
        _state.isLocked = false;
        _state.isOpen = true;
        GameUI.Instance.SetInteractionText("Porta Hackeada!");
        Debug.Log("Porta hackeada com sucesso!");
        // Reduz o tempo no timer, se houver referência
        if (timer != null)
        {
            timer.ReduceTime(hackTimeReduction);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
            GameUI.Instance.ToggleInteractionText(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            GameUI.Instance.ToggleInteractionText(false);
        }
    }
}
