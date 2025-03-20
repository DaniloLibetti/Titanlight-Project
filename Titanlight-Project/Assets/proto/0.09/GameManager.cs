using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    [Header("Grid Presets")]
    // Lista de presets para definir a quantidade de slots no grid (colunas x linhas)
    // Exemplo: do maior 7x6 até o menor 4x3
    public List<Vector2Int> gridPresets = new List<Vector2Int>()
    {
        new Vector2Int(7, 6),
        new Vector2Int(6, 5),
        new Vector2Int(5, 4),
        new Vector2Int(4, 3),
        new Vector2Int(7, 4)
    };

    [Header("Grid Settings")]
    // gridSize não é mais definido manualmente, será escolhido aleatoriamente a partir dos presets
    public Vector2Int gridSize = new Vector2Int(5, 5);
    public float roomWidth = 7f;
    public float roomHeight = 3.93f;

    [Header("Room Options")]
    public GameObject initialRoomPrefab;
    public List<RoomOption> roomOptions;

    [Serializable]
    public class RoomOption
    {
        public string roomName;
        public GameObject roomPrefab;
        [Range(0, 100)]
        public float spawnChance;
    }

    [Header("Door Settings")]
    private Dictionary<Vector2Int, HashSet<DoorDirection>> doors = new Dictionary<Vector2Int, HashSet<DoorDirection>>();

    private Dictionary<Vector2Int, Room> _rooms = new Dictionary<Vector2Int, Room>();
    private Vector2Int _currentRoomCoord;
    private Vector2Int _initialRoomCoord;
    private Vector3 _gridOffset;
    private Camera _mainCamera;
    private bool _isTransitioning = false;

    public event Action<Vector2Int> OnRoomChanged;

    protected override void Awake()
    {
        base.Awake();
        _mainCamera = Camera.main;

        // Seleciona aleatoriamente um preset para o grid
        gridSize = gridPresets[UnityEngine.Random.Range(0, gridPresets.Count)];
        Debug.Log($"[GameManager] Grid escolhido: {gridSize.x} x {gridSize.y}");

        // Cálculo do grid offset corrigido (centraliza a grade)
        _gridOffset = new Vector3(
            -(gridSize.x * roomWidth) / 2f + roomWidth / 2f,
            -(gridSize.y * roomHeight) / 2f + roomHeight / 2f,
            0
        );

        // Define aleatoriamente a coordenada da sala inicial com clamp para garantir valores válidos
        _initialRoomCoord = new Vector2Int(
            Mathf.Clamp(UnityEngine.Random.Range(0, gridSize.x), 0, gridSize.x - 1),
            Mathf.Clamp(UnityEngine.Random.Range(0, gridSize.y), 0, gridSize.y - 1)
        );

        GenerateGrid();
        SetCurrentRoom(_initialRoomCoord);

        if (Player.Instance != null)
        {
            Player.Instance.transform.position = GetRoomWorldPosition(_initialRoomCoord);
            Debug.Log($"[GameManager] Player posicionado no centro da sala inicial {_initialRoomCoord}");
        }
    }

    void GenerateGrid()
    {
        float totalChance = CalculateTotalSpawnChance();

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                Vector3 roomPosition = CalculateRoomPosition(x, y);

                GameObject roomObj = coord == _initialRoomCoord ?
                    InstantiateInitialRoom(coord, roomPosition) :
                    InstantiateRandomRoom(coord, roomPosition, totalChance);

                Room room = roomObj.GetComponent<Room>();
                _rooms.Add(coord, room);
            }
        }

        InitializeAllRooms();
    }

    float CalculateTotalSpawnChance()
    {
        float total = 0f;
        foreach (RoomOption option in roomOptions)
            total += option.spawnChance;
        return total;
    }

    Vector3 CalculateRoomPosition(int x, int y)
    {
        return new Vector3(x * roomWidth, y * roomHeight, 0) + _gridOffset;
    }

    GameObject InstantiateInitialRoom(Vector2Int coord, Vector3 position)
    {
        GameObject room = Instantiate(initialRoomPrefab, position, Quaternion.identity);
        Debug.Log($"[GameManager] Sala inicial criada em {coord}");
        return room;
    }

    GameObject InstantiateRandomRoom(Vector2Int coord, Vector3 position, float totalChance)
    {
        float randomValue = UnityEngine.Random.Range(0f, totalChance);
        float cumulative = 0f;

        foreach (RoomOption option in roomOptions)
        {
            cumulative += option.spawnChance;
            if (randomValue <= cumulative)
            {
                GameObject room = Instantiate(option.roomPrefab, position, Quaternion.identity);
                Debug.Log($"[GameManager] Sala '{option.roomName}' criada em {coord}");
                return room;
            }
        }
        // Fallback: caso nenhuma opção seja selecionada (não deve ocorrer se as chances forem > 0)
        GameObject fallbackRoom = Instantiate(roomOptions[0].roomPrefab, position, Quaternion.identity);
        Debug.Log($"[GameManager] Fallback: Sala '{roomOptions[0].roomName}' criada em {coord}");
        return fallbackRoom;
    }

    void InitializeAllRooms()
    {
        foreach (var pair in _rooms)
        {
            pair.Value.Initialize(pair.Key, roomWidth, roomHeight);
            Debug.Log($"[GameManager] Sala inicializada em {pair.Key}");
        }
    }

    public void RegisterDoor(Vector2Int roomCoord, DoorDirection direction)
    {
        if (!doors.ContainsKey(roomCoord))
        {
            doors[roomCoord] = new HashSet<DoorDirection>();
        }
        doors[roomCoord].Add(direction);
        Debug.Log($"[GameManager] Porta registrada na sala {roomCoord} com direção {direction}");
    }

    public bool IsDoorAccessible(Vector2Int roomCoord, DoorDirection direction)
    {
        return doors.ContainsKey(roomCoord) && doors[roomCoord].Contains(direction);
    }

    public Vector2Int GetCurrentRoomCoord() => _currentRoomCoord;

    public void SetCurrentRoom(Vector2Int coord)
    {
        _currentRoomCoord = coord;
        OnRoomChanged?.Invoke(coord);

        // Desativa os triggers de todas as salas
        foreach (var room in _rooms.Values)
        {
            room.SetActiveDoors(false);
        }
        // Ativa os triggers somente da sala atual
        Room currentRoom = GetRoom(coord);
        if (currentRoom != null)
        {
            currentRoom.SetActiveDoors(true);
        }

        UpdateCamera();
    }

    void UpdateCamera()
    {
        if (_rooms.TryGetValue(_currentRoomCoord, out Room currentRoom) && currentRoom.cameraSlot != null)
        {
            _mainCamera.transform.position = currentRoom.cameraSlot.position + new Vector3(0, 0, -10);
        }
    }

    public Room GetRoom(Vector2Int coord)
    {
        return _rooms.ContainsKey(coord) ? _rooms[coord] : null;
    }

    public IEnumerator MoveToRoomCoroutine(Vector2Int direction, Transform playerTransform, float transitionTime = 0.5f)
    {
        if (_isTransitioning)
        {
            Debug.Log("Transição já em andamento.");
            yield break;
        }

        Vector2Int newCoord = _currentRoomCoord + direction;
        DoorDirection doorDir = direction.ToDoorDirection();

        Debug.Log($"Tentando mover de {_currentRoomCoord} para {newCoord} usando direção {doorDir}");

        if (IsDoorAccessible(_currentRoomCoord, doorDir))
        {
            if (!_rooms.ContainsKey(newCoord))
            {
                Debug.LogError($"Sala {newCoord} não encontrada!");
                yield break;
            }

            _isTransitioning = true;
            // Posiciona o player no centro da nova sala
            playerTransform.position = GetRoomWorldPosition(newCoord);
            SetCurrentRoom(newCoord);
            yield return new WaitForSeconds(transitionTime);
            _isTransitioning = false;
            Debug.Log($"Transição concluída para a sala {newCoord}");
        }
        else
        {
            Debug.LogError($"Porta {doorDir} inacessível na sala {_currentRoomCoord}.");
        }
    }

    public void TryMoveToRoom(Vector2Int direction, Transform playerTransform)
    {
        StartCoroutine(MoveToRoomCoroutine(direction, playerTransform));
    }

    public Vector3 GetRoomWorldPosition(Vector2Int coord)
    {
        return new Vector3(coord.x * roomWidth, coord.y * roomHeight, 0) + _gridOffset;
    }
}
