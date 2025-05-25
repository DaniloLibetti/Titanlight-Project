using UnityEngine;

public enum DoorDirection { Up, Down, Left, Right }

public class DoorTrigger : MonoBehaviour
{
    public DoorDirection direction;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode hackKey = KeyCode.H;
    public float moveDistance = 3f;
    public float hackTimeReduction = 10f;
    public CountdownTimer timer;
    public DoorTrigger pairedDoor;

    // Estado da porta obtido da Room (caso não esteja pareada)
    private DoorState _state;

    // Este objeto será compartilhado entre as portas pareadas.
    // Se a porta não estiver pareada, sharedState equivale a _state.
    public DoorState sharedState;

    private bool _isPlayerInRange;

    // Raio para procurar a porta pareada (se necessário)
    public float pairingRadius = 2f;

    void Start()
    {

        if (timer == null)
            timer = FindObjectOfType<CountdownTimer>();


        // Obtém o estado da porta a partir da Room (por exemplo, travada e fechada inicialmente)
        Room room = GetComponentInParent<Room>();
        if (room != null)
        {
            _state = room.GetDoorState(direction);
            // Inicialmente, se não estiver pareada, o sharedState é o mesmo que _state.
            sharedState = _state;
            Debug.Log($"[{gameObject.name}] Estado obtido na sala {room.GridCoord} para a direção {direction}");
        }
        else
        {
            Debug.LogError("Room não encontrado no Start do DoorTrigger!");
        }

        // Tenta parear com outra porta próxima (se não houver pareamento já feito pelo GameManager)
        PairDoorWithOverlap();

        // Se a porta já estiver pareada, garanta que ambas compartilhem o mesmo estado
        if (pairedDoor != null)
        {
            if (pairedDoor.sharedState == null)
                pairedDoor.sharedState = sharedState;
            else
                sharedState = pairedDoor.sharedState;
        }
    }

    void PairDoorWithOverlap()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pairingRadius, LayerMask.GetMask("Doors"));
        foreach (var col in colliders)
        {
            if (col.gameObject == this.gameObject)
                continue;

            DoorTrigger otherDoor = col.GetComponent<DoorTrigger>();
            if (otherDoor != null)
            {
                Vector2 toOther = otherDoor.transform.position - transform.position;
                Vector2 expectedDir = DirectionToVector(direction);
                // Se o objeto estiver aproximadamente na direção esperada...
                if (Vector2.Dot(toOther.normalized, expectedDir) > 0.5f)
                {
                    pairedDoor = otherDoor;
                    otherDoor.pairedDoor = this;
                    Debug.Log($"[{gameObject.name}] Pareado com {otherDoor.gameObject.name}");
                    // Garanta que ambas compartilhem o mesmo estado
                    if (otherDoor.sharedState == null)
                        otherDoor.sharedState = sharedState;
                    else
                        sharedState = otherDoor.sharedState;
                    break;
                }
            }
        }
    }

    void Update()
    {
        if (!_isPlayerInRange) return;
        UpdateInterface();
        CheckInputs();
    }

    void UpdateInterface()
    {
        if (sharedState.isLocked)
            GameUI.Instance.SetInteractionText("[H] Hackear Porta");
        else if (!sharedState.isOpen)
            GameUI.Instance.SetInteractionText("Porta Quebrada");
        else
            GameUI.Instance.SetInteractionText("[E] Entrar");
    }

    void CheckInputs()
    {
        if (Input.GetKeyDown(hackKey))
        {
            TryHackDoor();
            SoundManager.PlaySound(SoundType.HACKING);
        }

        else if (Input.GetKeyDown(interactKey))
        {
            TryPassThrough();
            SoundManager.PlaySound(SoundType.DOOR);
        }

    }

    void TryPassThrough()
    {
        // Se a porta estiver aberta, o GameManager trata da transição (passagem pelo túnel)
        if (sharedState.isOpen && !sharedState.isLocked)
            GameManager.Instance.TryMoveThroughDoor(direction, moveDistance);
    }

    public void TryHackDoor()
    {
        if (!sharedState.isLocked)
        {
            Debug.Log($"[{gameObject.name}] Porta já desbloqueada.");
            return;
        }

        Debug.Log($"[{gameObject.name}] Tentando hackear a porta na direção {direction}.");
        // Desbloqueia esta porta; como o sharedState é o mesmo para as duas, ambas ficam atualizadas
        UnlockDoor();

        // Se houver porta pareada, não é necessário chamá-la separadamente, pois o estado é compartilhado
        timer?.ReduceTime(hackTimeReduction);
    }

    public void UnlockDoor()
    {
        if (!sharedState.isLocked)
            return;

        sharedState.isLocked = false;
        sharedState.isOpen = true;
        Debug.Log($"[{gameObject.name}] Porta desbloqueada na sala {GetRoomCoord()} na direção {direction}");

        // Registra essa porta no GameManager usando as coordenadas da sala
        Room room = GetComponentInParent<Room>();
        if (room != null)
        {
            GameManager.Instance.RegisterDoor(room.GridCoord, direction);
        }
        else
        {
            Debug.LogError("Room não encontrado ao desbloquear a porta!");
        }
    }

    Vector2Int GetRoomCoord()
    {
        Room room = GetComponentInParent<Room>();
        return room != null ? room.GridCoord : new Vector2Int(-1, -1);
    }

    private Vector2 DirectionToVector(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.Up: return Vector2.up;
            case DoorDirection.Down: return Vector2.down;
            case DoorDirection.Left: return Vector2.left;
            case DoorDirection.Right: return Vector2.right;
            default: return Vector2.zero;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
            GameUI.Instance.ToggleInteractionText(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            GameUI.Instance.ToggleInteractionText(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pairingRadius);
    }
}
