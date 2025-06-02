// DoorTrigger.cs
using UnityEngine;
using TMPro;
using Player.StateMachine; // para usar PlayerStateMachine na inscrição de eventos
using System.Collections.Generic;

public enum DoorDirection { Up, Down, Left, Right }

public class DoorTrigger : MonoBehaviour
{
    [Header("Door Settings")]
    public DoorDirection direction;
    public float moveDistance = 3f;
    public float hackTimeReduction = 10f;
    public CountdownTimer timer;
    public DoorTrigger pairedDoor;

    [Tooltip("Arraste aqui o shader que faz a troca de cor (vermelho/verde/rainbow/esperando)")]
    public Shader doorShader;

    [Tooltip("Texto de dica (TextMeshPro)")]
    public TextMeshProUGUI promptText;

    private DoorState _state;
    public DoorState sharedState;

    private bool _isPlayerInRange;
    private bool _hackingInProgress;
    private float _hackTimer;
    private const float HACK_DURATION = 2f;

    // Controla quais jogadores estão em alcance e prontos
    private HashSet<PlayerStateMachine> _playersInRange = new HashSet<PlayerStateMachine>();
    private HashSet<PlayerStateMachine> _playersReady = new HashSet<PlayerStateMachine>();

    [Header("Pairing Settings")]
    public float pairingRadius = 2f;

    // Render e material do shader
    private SpriteRenderer _spriteRenderer;
    private Material _doorMaterial;

    void OnEnable()
    {
        PlayerStateMachine.HackPressed += HandleHackPressed;
        PlayerStateMachine.HackReleased += HandleHackReleased;
        PlayerStateMachine.InteractPressed += HandleInteractPressed;
    }

    void OnDisable()
    {
        PlayerStateMachine.HackPressed -= HandleHackPressed;
        PlayerStateMachine.HackReleased -= HandleHackReleased;
        PlayerStateMachine.InteractPressed -= HandleInteractPressed;
    }

    void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null && doorShader != null)
        {
            _doorMaterial = new Material(doorShader);
            _spriteRenderer.material = _doorMaterial;
            _doorMaterial.SetFloat("_IsOpen", 0f);
            _doorMaterial.SetFloat("_IsHacking", 0f);
            _doorMaterial.SetFloat("_IsWaiting", 0f);
        }
        else if (_spriteRenderer == null)
            Debug.LogWarning($"[DoorTrigger] SpriteRenderer não encontrado em '{name}'.");
        else
            Debug.LogWarning($"[DoorTrigger] doorShader não atribuído em '{name}'.");
    }

    void Start()
    {
        if (timer == null)
            timer = FindObjectOfType<CountdownTimer>();

        var room = GetComponentInParent<Room>();
        if (room != null)
        {
            _state = room.GetDoorState(direction);
            sharedState = _state;
        }
        else
        {
            Debug.LogError($"[DoorTrigger] Room não encontrado em '{name}'.");
        }

        PairDoorWithOverlap();

        if (pairedDoor != null)
        {
            if (pairedDoor.sharedState == null)
                pairedDoor.sharedState = sharedState;
            else
                sharedState = pairedDoor.sharedState;
        }

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!_isPlayerInRange)
            return;

        UpdateInterface();

        if (_hackingInProgress)
        {
            _hackTimer += Time.deltaTime;
            if (_hackTimer >= HACK_DURATION)
            {
                _hackingInProgress = false;
                _hackTimer = 0f;
                UnlockDoor();
            }
        }
    }

    void UpdateInterface()
    {
        if (promptText == null)
            return;

        if (sharedState.isLocked)
        {
            promptText.text = _hackingInProgress
                ? "Hackeando…"
                : "Segure [Hack] por 2s para desbloquear";
        }
        else if (!sharedState.isOpen)
        {
            promptText.text = "Porta Quebrada";
        }
        else if (!AllPlayersReady())
        {
            promptText.text = "Pressione [Interact] para entrar (aguarde outro jogador)";
        }
        else
        {
            promptText.text = "";
        }

        promptText.gameObject.SetActive(true);
    }

    private void HandleHackPressed(PlayerStateMachine sender)
    {
        if (!_playersInRange.Contains(sender) || !sharedState.isLocked)
            return;

        _hackingInProgress = true;
        _hackTimer = 0f;
        _doorMaterial?.SetFloat("_IsHacking", 1f);
    }

    private void HandleHackReleased(PlayerStateMachine sender)
    {
        if (_hackingInProgress)
        {
            _hackingInProgress = false;
            _hackTimer = 0f;
            _doorMaterial?.SetFloat("_IsHacking", 0f);
        }
    }

    private void HandleInteractPressed(PlayerStateMachine sender)
    {
        if (!_playersInRange.Contains(sender) || sharedState.isLocked || !sharedState.isOpen)
            return;

        if (_playersReady.Contains(sender))
            return;

        _playersReady.Add(sender);

        if (_playersReady.Count == 1)
            _doorMaterial?.SetFloat("_IsWaiting", 1f);

        if (AllPlayersReady())
            TryPassThrough();
    }

    private bool AllPlayersReady()
    {
        // Precisamos de exatamente 2 prontos, e ambos ainda em alcance
        var p1 = PlayerStateMachine.Instance;
        var p2 = PlayerStateMachine.Instance2;
        return _playersReady.Count == 2
            && p1 != null && p2 != null
            && _playersReady.Contains(p1)
            && _playersReady.Contains(p2);
    }

    private void TryPassThrough()
    {
        _doorMaterial?.SetFloat("_IsHacking", 0f);
        _doorMaterial?.SetFloat("_IsWaiting", 0f);
        _doorMaterial?.SetFloat("_IsOpen", 1f);

        _playersReady.Clear();
        GameManager.Instance.TryMoveThroughDoor(direction, moveDistance);
        SoundManager.PlaySound(SoundType.DOOR);
    }

    public void UnlockDoor()
    {
        if (!sharedState.isLocked)
            return;

        sharedState.isLocked = false;
        sharedState.isOpen = true;
        timer?.ReduceTime(hackTimeReduction);
        SoundManager.PlaySound(SoundType.HACKING);

        _doorMaterial?.SetFloat("_IsHacking", 0f);
        _doorMaterial?.SetFloat("_IsWaiting", 0f);
        _doorMaterial?.SetFloat("_IsOpen", 0f);
    }

    void PairDoorWithOverlap()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            pairingRadius,
            LayerMask.GetMask("Doors")
        );
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            var other = hit.GetComponent<DoorTrigger>();
            if (other == null) continue;

            Vector2 toOther = other.transform.position - transform.position;
            Vector2 expected = DirectionToVector(direction);
            if (Vector2.Dot(toOther.normalized, expected) > 0.5f)
            {
                pairedDoor = other;
                other.pairedDoor = this;
                if (other.sharedState == null)
                    other.sharedState = sharedState;
                else
                    sharedState = other.sharedState;
                break;
            }
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
        if (!other.CompareTag("Player")) return;
        var psm = other.GetComponent<PlayerStateMachine>();
        if (psm != null)
        {
            _playersInRange.Add(psm);
            _isPlayerInRange = true;
        }
        if (promptText != null)
            promptText.gameObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var psm = other.GetComponent<PlayerStateMachine>();
        if (psm != null)
        {
            _playersInRange.Remove(psm);
            _playersReady.Remove(psm);
        }

        if (_playersReady.Count < 2)
            _doorMaterial?.SetFloat("_IsWaiting", 0f);

        if (_playersInRange.Count == 0)
        {
            _isPlayerInRange = false;
            if (promptText != null)
                promptText.gameObject.SetActive(false);
        }

        // Atualiza cor final (aberta/fechada)
        _doorMaterial?.SetFloat("_IsHacking", 0f);
        _doorMaterial?.SetFloat("_IsWaiting", (_playersReady.Count >= 1 && _playersInRange.Count >= 1) ? 1f : 0f);
        _doorMaterial?.SetFloat("_IsOpen", sharedState.isOpen ? 1f : 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pairingRadius);
    }
}
