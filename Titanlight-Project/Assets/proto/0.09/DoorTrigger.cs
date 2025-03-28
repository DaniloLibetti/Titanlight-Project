using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    // Configura��es b�sicas da porta
    public DoorDirection direction; // Dire��o que a porta est� virada
    public KeyCode interactKey = KeyCode.E; // Tecla para usar porta destrancada
    public KeyCode hackKey = KeyCode.H; // Tecla para hackear porta trancada

    // Valores ajust�veis
    public float moveDistance = 3f; // Quanto o jogador se move ao passar
    public float hackTimeReduction = 10f; // Tempo perdido ao hackear

    // Refer�ncia para o timer (pode ser arrastado no Inspector)
    public CountdownTimer timer;

    // Estado atual da porta
    private DoorState _state;
    private bool _isPlayerInRange; // Se jogador est� perto

    void Start()
    {
        // Tenta achar o timer automaticamente se n�o tiver
        if (timer == null)
        {
            timer = FindObjectOfType<CountdownTimer>();
        }

        // Pega o estado da porta da sala pai
        Room room = GetComponentInParent<Room>();
        if (room != null)
        {
            _state = room.GetDoorState(direction);
        }
    }

    void Update()
    {
        if (!_isPlayerInRange) return; // S� funciona com jogador perto

        AtualizaInterface(); // Mostra textos na UI
        VerificaInputs(); // Checa teclas pressionadas
    }

    void AtualizaInterface()
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

    void VerificaInputs()
    {
        // Tecla H para hackear porta trancada
        if (Input.GetKeyDown(hackKey) && _state.isLocked)
        {
            HackearPorta();
        }
        // Tecla E para usar porta destrancada
        else if (Input.GetKeyDown(interactKey))
        {
            TentarPassarPorta();
        }
    }

    void TentarPassarPorta()
    {
        if (_state.isOpen && !_state.isLocked)
        {
            // Manda o jogador se mover
            GameManager.Instance.TryMoveThroughDoor(direction, moveDistance);
        }
    }

    public void HackearPorta()
    {
        _state.isLocked = false; // Destranca
        _state.isOpen = true; // Abre
        GameUI.Instance.SetInteractionText("Porta Hackeada!"); // Feedback

        // Reduz tempo se tiver timer
        if (timer != null)
        {
            timer.ReduceTime(hackTimeReduction);
        }
    }

    // Quando jogador entra na �rea
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
            GameUI.Instance.ToggleInteractionText(true); // Mostra texto
        }
    }

    // Quando jogador sai da �rea
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            GameUI.Instance.ToggleInteractionText(false); // Esconde texto
        }
    }
}