// DoorTrigger.cs
using UnityEngine;

public enum DoorDirection { Up, Down, Left, Right }

public class DoorTrigger : MonoBehaviour
{
    public DoorDirection direction;
    public float moveDistance = 3f;
    public float hackTimeReduction = 10f;
    public CountdownTimer timer;
    public DoorTrigger pairedDoor;

    private DoorState _state;
    public DoorState sharedState;

    private bool _isPlayerInRange;
    public float pairingRadius = 2f;

    void Start()
    {
        if (timer == null)
            timer = FindObjectOfType<CountdownTimer>();

        Room room = GetComponentInParent<Room>();
        if (room != null)
        {
            _state = room.GetDoorState(direction);
            sharedState = _state;
        }
        else
        {
            Debug.LogError("Room não encontrado no Start do DoorTrigger!");
        }

        PairDoorWithOverlap();

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
                if (Vector2.Dot(toOther.normalized, expectedDir) > 0.5f)
                {
                    pairedDoor = otherDoor;
                    otherDoor.pairedDoor = this;
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
    }

    void UpdateInterface()
    {
        if (sharedState.isLocked)
            GameUI.Instance.SetInteractionText("Pressione [Hack] para desbloquear");
        else if (!sharedState.isOpen)
            GameUI.Instance.SetInteractionText("Porta Quebrada");
        else
            GameUI.Instance.SetInteractionText("Pressione [Interact] para atravessar");
    }

    // Atravessa somente se já estiver aberta
    public void Interact()
    {
        Debug.Log("DoorTrigger.Interact chamado em " + gameObject.name);
        if (!_isPlayerInRange) return;

        if (sharedState.isOpen && !sharedState.isLocked)
        {
            TryPassThrough();
        }
        else
        {
            Debug.Log("Porta fechada ou travada; não pode atravessar.");
        }
    }

    // Desbloqueia porta
    public void Hack()
    {
        Debug.Log("DoorTrigger.Hack chamado em " + gameObject.name);
        if (!_isPlayerInRange) return;

        if (sharedState.isLocked)
            TryHackDoor();
        else
            Debug.Log("Porta já desbloqueada.");
    }

    void TryPassThrough()
    {
        Debug.Log("Passando pela porta " + gameObject.name);
        GameManager.Instance.TryMoveThroughDoor(direction, moveDistance);
        SoundManager.PlaySound(SoundType.DOOR);
    }

    public void TryHackDoor()
    {
        if (!sharedState.isLocked) return;
        Debug.Log("Tentando hackear porta " + gameObject.name);
        UnlockDoor();
        timer?.ReduceTime(hackTimeReduction);
    }

    public void UnlockDoor()
    {
        if (!sharedState.isLocked) return;
        sharedState.isLocked = false;
        sharedState.isOpen = true;
        Debug.Log($"Porta {gameObject.name} desbloqueada");

        Room room = GetComponentInParent<Room>();
        if (room != null)
        {
            GameManager.Instance.RegisterDoor(room.GridCoord, direction);
            SoundManager.PlaySound(SoundType.HACKING);
        }
        else
        {
            Debug.LogError("Room não encontrado ao desbloquear a porta!");
        }
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
            Debug.Log("Player entrou no alcance da porta " + gameObject.name);
            _isPlayerInRange = true;
            GameUI.Instance.ToggleInteractionText(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player saiu do alcance da porta " + gameObject.name);
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
