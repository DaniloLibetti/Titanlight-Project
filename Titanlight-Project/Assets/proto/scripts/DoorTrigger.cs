// DoorTrigger.cs
using UnityEngine;
using TMPro;
using Player.StateMachine;  // Para eventos de Hack e Interact
using System.Collections.Generic;
using System;

public enum DoorDirection { Up, Down, Left, Right }

[RequireComponent(typeof(Collider2D))]
public class DoorTrigger : MonoBehaviour
{
    [Header("Door Settings")]
    public DoorDirection direction;
    public float moveDistance = 3f;
    public float hackTimeReduction = 10f;
    public CountdownTimer timer;
    public DoorTrigger pairedDoor;

    [Header("Lock Chance")]
    [Range(0f, 1f)]
    [Tooltip("Chance desta porta começar trancada")]
    public float lockChance = 0.5f;

    [Tooltip("Shader para troca de cor")]
    public Shader doorShader;

    [Tooltip("Texto de dica (TextMeshPro)")]
    public TextMeshProUGUI promptText;

    private DoorState sharedState;
    private bool _lockInitialized = false;

    private bool _isPlayerInRange;
    private bool _hackingInProgress;
    private float _hackTimer;
    private const float HACK_DURATION = 2f;

    private HashSet<PlayerStateMachine> _playersInRange = new();
    private HashSet<PlayerStateMachine> _playersReady = new();

    [Header("Pairing Settings")]
    public float pairingRadius = 2f;

    // Render
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
        if (doorShader != null && _spriteRenderer != null)
        {
            // Cria material único para esta porta
            _doorMaterial = new Material(doorShader);
            _spriteRenderer.material = _doorMaterial;

            // Inicializa flags
            _doorMaterial.SetFloat("_IsOpen", 0f);
            _doorMaterial.SetFloat("_IsHacking", 0f);
            _doorMaterial.SetFloat("_IsWaiting", 0f);
        }
    }

    void Start()
    {
        if (timer == null)
            timer = FindObjectOfType<CountdownTimer>();

        // Obtém DoorState da sala
        var room = GetComponentInParent<Room>();
        sharedState = room != null
            ? room.GetDoorState(direction)
            : new DoorState();

        // Emparelha automaticamente se não definido
        if (pairedDoor == null)
            PairDoorWithOverlap();

        // Sincroniza sharedState em ambas as portas
        if (pairedDoor != null)
            pairedDoor.sharedState = sharedState;

        // Inicializa lock apenas uma vez
        if (!_lockInitialized)
            InitializeLock();

        // Esconde prompt inicialmente
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Atualiza shader de acordo com estados
        if (_doorMaterial != null)
        {
            _doorMaterial.SetFloat("_IsOpen", sharedState.isOpen ? 1f : 0f);
            _doorMaterial.SetFloat("_IsHacking", _hackingInProgress ? 1f : 0f);
            _doorMaterial.SetFloat("_IsWaiting", _playersReady.Count > 0 ? 1f : 0f);
        }

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
        if (promptText == null) return;

        if (sharedState.isLocked)
            promptText.text = _hackingInProgress
                ? "Hackeando…"
                : "Segure [Hack] por 2s para desbloquear";
        else if (!sharedState.isOpen)
            promptText.text = "Porta Quebrada";
        else if (!AllPlayersReady())
            promptText.text = "Pressione [Interact] para entrar";
        else
            promptText.text = "";

        promptText.gameObject.SetActive(true);
    }

    private void HandleHackPressed(PlayerStateMachine sender)
    {
        if (!_playersInRange.Contains(sender) || !sharedState.isLocked)
            return;
        _hackingInProgress = true;
        _hackTimer = 0f;
    }

    private void HandleHackReleased(PlayerStateMachine sender)
    {
        if (!_hackingInProgress) return;
        _hackingInProgress = false;
        _hackTimer = 0f;
    }

    private void HandleInteractPressed(PlayerStateMachine sender)
    {
        if (!_playersInRange.Contains(sender)
            || sharedState.isLocked
            || !sharedState.isOpen)
            return;

        if (_playersReady.Add(sender) &&
            _playersReady.Count == (PlayerManager.Instance.isMultiplayer ? 2 : 1))
        {
            TryPassThrough();
        }
    }

    private bool AllPlayersReady()
    {
        if (!PlayerManager.Instance.isMultiplayer)
            return _playersReady.Contains(PlayerStateMachine.Instance);

        return _playersReady.Contains(PlayerStateMachine.Instance)
            && _playersReady.Contains(PlayerStateMachine.Instance2);
    }

    private void TryPassThrough()
    {
        _playersReady.Clear();
        PlayerManager.Instance.TryMoveThroughDoor(direction, moveDistance);
        SoundManager.PlaySound(SoundType.DOOR);
    }

    public void UnlockDoor()
    {
        if (!sharedState.isLocked) return;
        sharedState.isLocked = false;
        sharedState.isOpen = true;
        timer?.ReduceTime(hackTimeReduction);
        SoundManager.PlaySound(SoundType.HACKING);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")
            && other.TryGetComponent<PlayerStateMachine>(out var psm))
        {
            _playersInRange.Add(psm);
            _isPlayerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")
            && other.TryGetComponent<PlayerStateMachine>(out var psm))
        {
            _playersInRange.Remove(psm);
            _playersReady.Remove(psm);
        }

        if (_playersReady.Count == 0 && _doorMaterial != null)
            _doorMaterial.SetFloat("_IsWaiting", 0f);

        _isPlayerInRange = _playersInRange.Count > 0;
    }

    private void InitializeLock()
    {
        _lockInitialized = true;
        bool a = UnityEngine.Random.value < lockChance;
        bool b = UnityEngine.Random.value < lockChance;
        bool locked = (a == b) ? a : (UnityEngine.Random.value < 0.5f);
        sharedState.isLocked = locked;
        sharedState.isOpen = !locked;
    }

    private void PairDoorWithOverlap()
    {
        var hits = Physics2D.OverlapCircleAll(
            transform.position,
            pairingRadius,
            LayerMask.GetMask("Doors")
        );

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            var dirVec = (hit.transform.position - transform.position).normalized;
            if (Vector2.Dot(dirVec, DirectionToVector(direction)) < 0.5f)
                continue;

            var other = hit.GetComponent<DoorTrigger>();
            if (other == null) continue;

            pairedDoor = other;
            other.pairedDoor = this;
            other.sharedState = sharedState;
            break;
        }
    }

    private Vector2 DirectionToVector(DoorDirection dir) => dir switch
    {
        DoorDirection.Up => Vector2.up,
        DoorDirection.Down => Vector2.down,
        DoorDirection.Left => Vector2.left,
        DoorDirection.Right => Vector2.right,
        _ => Vector2.zero,
    };

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pairingRadius);
    }
}
