using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("Grid Parameters")]
    [SerializeField]
    private Vector2Int[] _gridPresets = {
        new Vector2Int(7, 6),
        new Vector2Int(6, 5),
        new Vector2Int(5, 4),
        new Vector2Int(4, 4),
        new Vector2Int(4, 3)
    };

    public float roomWidth = 7f;
    public float roomHeight = 3.93f;
    public Vector2Int GridSize { get; private set; }
    private Vector3 _gridOffset;

    [Header("Room Configuration")]
    public GameObject initialRoomPrefab;
    public List<RoomOption> roomOptions;

    private Dictionary<Vector2Int, Room> _rooms = new Dictionary<Vector2Int, Room>();
    private Vector2Int _currentRoomCoord;
    private Vector2Int _initialRoomCoord;

    [Header("Door Management")]
    // Dicionário que registra as direções de portas acessíveis para cada sala
    private Dictionary<Vector2Int, HashSet<DoorDirection>> _doors = new Dictionary<Vector2Int, HashSet<DoorDirection>>();

    [Header("Player Settings")]
    public GameObject playerPrefab;
    private Camera _mainCamera;
    private bool _isTransitioning = false;
    public event Action<Vector2Int> OnRoomChanged;

    [Serializable]
    public class RoomOption
    {
        public string roomName;
        public GameObject roomPrefab;
        [Range(0, 100)]
        public float spawnChance;
    }

    // ===============================
    // NOVA PARTE: Game Over Settings
    // ===============================
    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverPanel; // Painel com a opção de voltar para a MoonBox
    // Esse painel deverá estar desativado por padrão no inspetor.

    protected override void Awake()
    {
        base.Awake();
        InitializeGame();
    }

    private void InitializeGame()
    {
        _mainCamera = Camera.main;
        SetupGrid();
        GenerateWorld();
        // Depois de gerar as salas, pareia as portas
        PairDoors();
        InstantiatePlayer();

        // Sempre que uma nova run inicia, tenta resetar o timer (caso exista na cena)
        CountdownTimer timer = FindObjectOfType<CountdownTimer>();
        if (timer != null)
        {
            timer.ResetTimer();
        }
    }

    private void SetupGrid()
    {
        GridSize = _gridPresets[UnityEngine.Random.Range(0, _gridPresets.Length)];
        _gridOffset = new Vector3(
            -(GridSize.x * roomWidth) / 2f + roomWidth / 2f,
            -(GridSize.y * roomHeight) / 2f + roomHeight / 2f,
            0
        );
    }

    private void GenerateWorld()
    {
        _initialRoomCoord = new Vector2Int(
            UnityEngine.Random.Range(0, GridSize.x),
            UnityEngine.Random.Range(0, GridSize.y)
        );

        float totalChance = CalculateTotalSpawnChance();

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                CreateRoom(coord, totalChance);
            }
        }
        SetCurrentRoom(_initialRoomCoord);
    }

    private void CreateRoom(Vector2Int coord, float totalChance)
    {
        Vector3 position = new Vector3(
            coord.x * roomWidth,
            coord.y * roomHeight,
            0
        ) + _gridOffset;

        GameObject prefab = (coord == _initialRoomCoord) ?
            initialRoomPrefab :
            SelectRandomRoomPrefab(totalChance);

        Room room = Instantiate(prefab, position, Quaternion.identity).GetComponent<Room>();
        _rooms.Add(coord, room);
        room.Initialize(coord, roomWidth, roomHeight);
    }

    private GameObject SelectRandomRoomPrefab(float totalChance)
    {
        float randomValue = UnityEngine.Random.Range(0f, totalChance);
        float cumulative = 0f;

        foreach (RoomOption option in roomOptions)
        {
            cumulative += option.spawnChance;
            if (randomValue <= cumulative)
                return option.roomPrefab;
        }
        return roomOptions[0].roomPrefab;
    }

    private float CalculateTotalSpawnChance()
    {
        float total = 0f;
        foreach (RoomOption option in roomOptions)
            total += option.spawnChance;
        return total;
    }

    private void InstantiatePlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab não definido no GameManager!");
            return;
        }

        Vector3 spawnPos = _mainCamera.transform.position;
        spawnPos.z = 0;
        GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        // Se o player possui o componente Health, inscreve no evento onDeath para:
        // 1 - Ativar o painel de Game Over.
        // 2 - Notificar o CountdownTimer para parar e desaparecer.
        Health playerHealth = playerObj.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.onDeath.AddListener(HandlePlayerDeath);

            CountdownTimer timer = FindObjectOfType<CountdownTimer>();
            if (timer != null)
            {
                playerHealth.onDeath.AddListener(() =>
                {
                    timer.OnPlayerDeath();
                });
            }
            else
            {
                Debug.LogWarning("CountdownTimer não encontrado na cena!");
            }
        }
        else
        {
            Debug.LogWarning("Componente Health não encontrado no prefab do Player!");
        }
    }

    // Método chamado quando o player morre
    private void HandlePlayerDeath()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameOverPanel não foi atribuído no inspetor!");
        }
    }

    // Método chamado pelo botão da tela de Game Over para voltar para a MoonBox
    public void ReturnToMoonBox()
    {
        SceneManager.LoadScene("MoonBox");
    }

    // Chamado pelos scripts de porta para registrar uma porta desbloqueada
    public void RegisterDoor(Vector2Int roomCoord, DoorDirection direction)
    {
        if (!_doors.ContainsKey(roomCoord))
            _doors[roomCoord] = new HashSet<DoorDirection>();

        if (!_doors[roomCoord].Contains(direction))
        {
            _doors[roomCoord].Add(direction);
            Debug.Log($"[GameManager] Porta {direction} na sala {roomCoord} registrada como acessível.");
        }
    }

    // Verifica se a porta de uma dada direção na sala está registrada como acessível
    public bool IsDoorAccessible(Vector2Int roomCoord, DoorDirection direction)
    {
        return _doors.ContainsKey(roomCoord) && _doors[roomCoord].Contains(direction);
    }

    public Vector3 GetRoomWorldPosition(Vector2Int coord)
    {
        return new Vector3(
            coord.x * roomWidth,
            coord.y * roomHeight,
            0
        ) + _gridOffset;
    }

    public Room GetRoom(Vector2Int coord)
    {
        return _rooms.ContainsKey(coord) ? _rooms[coord] : null;
    }

    public Vector2Int GetCurrentRoomCoord()
    {
        return _currentRoomCoord;
    }

    public void SetCurrentRoom(Vector2Int coord)
    {
        _currentRoomCoord = coord;
        OnRoomChanged?.Invoke(coord);

        // Desativa as portas de todas as salas e ativa somente as da sala atual
        foreach (var room in _rooms.Values)
            room.SetActiveDoors(false);

        if (_rooms.TryGetValue(coord, out Room currentRoom))
            currentRoom.SetActiveDoors(true);

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (_rooms.TryGetValue(_currentRoomCoord, out Room room) && room.cameraSlot != null)
            _mainCamera.transform.position = room.cameraSlot.position + new Vector3(0, 0, -10);
    }

    // Tenta mover o jogador através de uma porta, verificando se a porta foi registrada como desbloqueada
    public void TryMoveThroughDoor(DoorDirection direction, float moveDistance)
    {
        Vector2Int dirVector = direction.ToVector();
        StartCoroutine(TransitionThroughDoor(dirVector, moveDistance));
    }

    private IEnumerator TransitionThroughDoor(Vector2Int direction, float moveDistance)
    {
        if (_isTransitioning)
            yield break;
        _isTransitioning = true;

        Vector2Int newCoord = _currentRoomCoord + direction;
        if (IsDoorAccessible(_currentRoomCoord, direction.ToDoorDirection()))
        {
            Collider2D playerCollider = Player.Instance.GetComponent<Collider2D>();
            playerCollider.enabled = false;

            Vector3 startPos = Player.Instance.transform.position;
            Vector3 targetPos = startPos + (Vector3)(Vector2)direction * moveDistance;

            yield return SmoothTransition(playerCollider, startPos, targetPos);
            SetCurrentRoom(newCoord);
        }
        else
        {
            Debug.LogWarning($"[GameManager] Porta {direction.ToDoorDirection()} na sala {_currentRoomCoord} não está acessível!");
        }
        _isTransitioning = false;
    }

    private IEnumerator SmoothTransition(Collider2D collider, Vector3 start, Vector3 target)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Player.Instance.transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Player.Instance.transform.position = target;
        collider.enabled = true;
    }

    // Método para parear as portas entre salas vizinhas
    private void PairDoors()
    {
        foreach (var roomEntry in _rooms)
        {
            Vector2Int coord = roomEntry.Key;
            Room room = roomEntry.Value;

            // Pareia porta de cima com a porta de baixo da sala acima
            var doorUp = room.GetDoorTrigger(DoorDirection.Up);
            if (doorUp != null)
            {
                Vector2Int neighborCoord = coord + Vector2Int.up;
                Room neighbor = GetRoom(neighborCoord);
                if (neighbor != null)
                {
                    var neighborDoor = neighbor.GetDoorTrigger(DoorDirection.Down);
                    if (neighborDoor != null)
                    {
                        doorUp.pairedDoor = neighborDoor;
                        neighborDoor.pairedDoor = doorUp;
                        Debug.Log($"[GameManager] Pareado: Sala {coord} (porta Up) com Sala {neighborCoord} (porta Down).");
                    }
                }
            }

            // Pareia porta da direita com a porta da esquerda da sala à direita
            var doorRight = room.GetDoorTrigger(DoorDirection.Right);
            if (doorRight != null)
            {
                Vector2Int neighborCoord = coord + Vector2Int.right;
                Room neighbor = GetRoom(neighborCoord);
                if (neighbor != null)
                {
                    var neighborDoor = neighbor.GetDoorTrigger(DoorDirection.Left);
                    if (neighborDoor != null)
                    {
                        doorRight.pairedDoor = neighborDoor;
                        neighborDoor.pairedDoor = doorRight;
                        Debug.Log($"[GameManager] Pareado: Sala {coord} (porta Right) com Sala {neighborCoord} (porta Left).");
                    }
                }
            }

            // Pareia porta de baixo com a porta de cima da sala abaixo
            var doorDown = room.GetDoorTrigger(DoorDirection.Down);
            if (doorDown != null)
            {
                Vector2Int neighborCoord = coord + Vector2Int.down;
                Room neighbor = GetRoom(neighborCoord);
                if (neighbor != null)
                {
                    var neighborDoor = neighbor.GetDoorTrigger(DoorDirection.Up);
                    if (neighborDoor != null)
                    {
                        doorDown.pairedDoor = neighborDoor;
                        neighborDoor.pairedDoor = doorDown;
                        Debug.Log($"[GameManager] Pareado: Sala {coord} (porta Down) com Sala {neighborCoord} (porta Up).");
                    }
                }
            }

            // Pareia porta da esquerda com a porta da direita da sala à esquerda
            var doorLeft = room.GetDoorTrigger(DoorDirection.Left);
            if (doorLeft != null)
            {
                Vector2Int neighborCoord = coord + Vector2Int.left;
                Room neighbor = GetRoom(neighborCoord);
                if (neighbor != null)
                {
                    var neighborDoor = neighbor.GetDoorTrigger(DoorDirection.Right);
                    if (neighborDoor != null)
                    {
                        doorLeft.pairedDoor = neighborDoor;
                        neighborDoor.pairedDoor = doorLeft;
                        Debug.Log($"[GameManager] Pareado: Sala {coord} (porta Left) com Sala {neighborCoord} (porta Right).");
                    }
                }
            }
        }
    }
}
