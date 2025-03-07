using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Vector2Int roomCoord;
    public DoorDirection direction;
    public KeyCode interactKey = KeyCode.E;
    private bool _isPlayerInTrigger = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = true;
            Debug.Log($"[DoorTrigger] Jogador entrou no trigger da porta {direction} na sala {roomCoord}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInTrigger = false;
            Debug.Log($"[DoorTrigger] Jogador saiu do trigger da porta {direction} na sala {roomCoord}");
        }
    }

    private void Update()
    {
        if (_isPlayerInTrigger && Input.GetKeyDown(interactKey))
        {
            Debug.Log($"[DoorTrigger] Interação detectada na porta {direction} na sala {roomCoord}");
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager.Instance é nulo!");
                return;
            }

            if (Player.Instance == null)
            {
                Debug.LogError("Player.Instance é nulo!");
                return;
            }

            Room room = GameManager.Instance.GetRoom(roomCoord);
            if (room == null)
            {
                Debug.LogError($"Sala {roomCoord} não encontrada!");
                return;
            }

            DoorState state = room.GetDoorState(direction);
            if (state == null)
            {
                Debug.LogError($"Porta {direction} não existe na sala {roomCoord}!");
                return;
            }

            if (state.isOpen && !state.isLocked)
            {
                Debug.Log($"[DoorTrigger] Passando pela porta {direction} na sala {roomCoord}");
                GameManager.Instance.TryMoveToRoom(direction.ToVector(), Player.Instance.transform);
            }
            else
            {
                Debug.Log($"[DoorTrigger] Porta {direction} na sala {roomCoord} está fechada ou trancada.");
            }
        }
    }
}
