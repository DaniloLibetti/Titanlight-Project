using UnityEngine;
using TMPro;
using Player.StateMachine;
using System.Collections.Generic;
using DungeonSystem;

public enum DoorDirection { Up, Down, Left, Right }

[RequireComponent(typeof(Collider2D))]
public class DoorTrigger : MonoBehaviour
{
    [Header("Door Settings")]
    public DoorDirection direction;
    public float hackTimeReduction = 10f;
    public CountdownTimer timer;
    public DoorTrigger pairedDoor;

    [Header("Spawn Points")]
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    [Header("Lock Chance")]
    [Range(0f, 1f)]
    [Tooltip("Chance desta porta começar trancada")]
    public float lockChance = 0.5f;

    [Tooltip("Shader para troca de cor")]
    public Shader doorShader;

    [Tooltip("Texto de dica (TextMeshPro)")]
    public TextMeshProUGUI promptText;

    [HideInInspector]
    public DoorState sharedState;

    private bool _lockInitialized = false;
    private bool _isPlayerInRange;
    private bool _hackingInProgress;
    private float _hackTimer;
    private const float HACK_DURATION = 2f;
    private bool _isLocked;

    private HashSet<PlayerStateMachine> _playersInRange = new HashSet<PlayerStateMachine>();
    private HashSet<PlayerStateMachine> _playersReady = new HashSet<PlayerStateMachine>();

    [Header("Pairing Settings")]
    public float pairingRadius = 2f;

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
            _doorMaterial = new Material(doorShader);
            _spriteRenderer.material = _doorMaterial;
            _doorMaterial.SetFloat("_IsOpen", 0f);
            _doorMaterial.SetFloat("_IsHacking", 0f);
            _doorMaterial.SetFloat("_IsWaiting", 0f);
        }
    }

    [System.Obsolete]
    void Start()
    {
        if (timer == null)
            timer = FindObjectOfType<CountdownTimer>();

        var room = GetComponentInParent<Room>();
        if (room != null)
        {
            sharedState = GameManager.Instance.GetDoorState(room.RoomCoord, direction);
        }
        else
        {
            sharedState = new DoorState();
        }

        if (pairedDoor == null)
            PairDoorWithOverlap();

        if (pairedDoor != null)
            pairedDoor.sharedState = sharedState;

        if (!_lockInitialized)
            InitializeLock();

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
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

        if (_isLocked)
            promptText.text = _hackingInProgress
                ? "Hackeando…"
                : "Segure [Hack] por 2s para desbloquear";
        else if (!sharedState.isOpen)
            promptText.text = "Porta Quebrada";
        else if (!AllPlayersReady())
            promptText.text = "Pressione [Interact] para entrar";
        else
            promptText.text = string.Empty;

        promptText.gameObject.SetActive(true);
    }

    private void HandleHackPressed(PlayerStateMachine sender)
    {
        if (!_playersInRange.Contains(sender) || !_isLocked)
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
            || _isLocked
            || !sharedState.isOpen)
            return;

        int requiredPlayers = GameManager.Instance.IsMultiplayer ? 2 : 1;

        if (_playersReady.Add(sender) &&
            _playersReady.Count == requiredPlayers)
        {
            TryPassThrough();
        }
    }

    private bool AllPlayersReady()
    {
        if (!GameManager.Instance.IsMultiplayer)
            return _playersReady.Contains(GetPlayerStateMachine(1));

        return _playersReady.Contains(GetPlayerStateMachine(1)) &&
               (PlayerManager.Instance.Player2 == null ||
                _playersReady.Contains(GetPlayerStateMachine(2)));
    }

    private PlayerStateMachine GetPlayerStateMachine(int playerIndex)
    {
        if (playerIndex == 1 && PlayerManager.Instance.Player1 != null)
            return PlayerManager.Instance.Player1.GetComponent<PlayerStateMachine>();

        if (playerIndex == 2 && PlayerManager.Instance.Player2 != null)
            return PlayerManager.Instance.Player2.GetComponent<PlayerStateMachine>();

        return null;
    }

    private void TryPassThrough()
    {
        _playersReady.Clear();

        if (pairedDoor == null)
        {
            Debug.LogError("No paired door found!");
            return;
        }

        if (pairedDoor.player1SpawnPoint == null)
        {
            Debug.LogError("Paired door has no spawn point for Player 1!");
            return;
        }

        var targetRoom = pairedDoor.GetComponentInParent<Room>();
        if (targetRoom == null)
        {
            Debug.LogError("Paired door has no parent room!");
            return;
        }

        var entryDirection = GetOppositeDirection(pairedDoor.direction);

        GameManager.Instance.SetCurrentRoom(targetRoom.RoomCoord, entryDirection);

        var player1 = PlayerManager.Instance.Player1;
        if (player1 != null)
        {
            player1.transform.position = pairedDoor.player1SpawnPoint.position;
            ResetPlayerPhysics(player1);
        }

        if (GameManager.Instance.IsMultiplayer)
        {
            var player2 = PlayerManager.Instance.Player2;
            if (player2 != null)
            {
                var player2Position = pairedDoor.player2SpawnPoint != null
                    ? pairedDoor.player2SpawnPoint.position
                    : pairedDoor.player1SpawnPoint.position + new Vector3(1.5f, 0f, 0f);

                player2.transform.position = player2Position;
                ResetPlayerPhysics(player2);
            }
        }

        SoundManager.PlaySound(SoundType.DOOR);
    }

    private void ResetPlayerPhysics(GameObject player)
    {
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private DoorDirection GetOppositeDirection(DoorDirection dir)
    {
        return dir switch
        {
            DoorDirection.Up => DoorDirection.Down,
            DoorDirection.Down => DoorDirection.Up,
            DoorDirection.Left => DoorDirection.Right,
            DoorDirection.Right => DoorDirection.Left,
        };
    }

    public void UnlockDoor()
    {
        if (!_isLocked) return;

        _isLocked = false;
        sharedState.isOpen = true;

        if (pairedDoor != null)
        {
            pairedDoor._isLocked = false;
            pairedDoor.sharedState.isOpen = true;
        }

        timer?.ReduceTime(hackTimeReduction);
        SoundManager.PlaySound(SoundType.HACKING);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<PlayerStateMachine>(out var psm))
        {
            _playersInRange.Add(psm);
            _isPlayerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<PlayerStateMachine>(out var psm))
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
        bool a = Random.value < lockChance;
        bool b = Random.value < lockChance;
        bool locked = (a == b) ? a : (Random.value < 0.5f);

        _isLocked = locked;
        sharedState.isOpen = !locked;

        // Sincronizar com porta pareada
        if (pairedDoor != null)
        {
            pairedDoor._isLocked = _isLocked;
            pairedDoor.sharedState.isOpen = !_isLocked;
            pairedDoor._lockInitialized = true;
        }
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

        if (player1SpawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(player1SpawnPoint.position, 0.2f);
            Gizmos.DrawLine(transform.position, player1SpawnPoint.position);
        }

        if (player2SpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(player2SpawnPoint.position, 0.2f);
            Gizmos.DrawLine(transform.position, player2SpawnPoint.position);
        }
    }
}