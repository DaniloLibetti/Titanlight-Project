using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : Singleton<GameManager>
{
    //vai definir o tamanho da grade mas ainda tenho que achar um jeito dele nao preencher toda grade, para ficar mai tipo moonlighter
    [Header("Grid Settings")]
    public Vector2Int gridSize = new Vector2Int(5, 5);
    public float roomWidth = 7f;
    public float roomHeight = 3.93f;

    //as opcoes de sala, tem uma inicial que sempre so tem uma, e outros tipos de sala e a chances de aparecer.
    [Header("Room Options")]
    public GameObject initialRoomPrefab;
    public List<RoomOption> roomOptions;

    //classe dentro da classe preciso disso para aparecer no inspetor (sem MonoBehaviour)
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
    private Vector2Int _initialRoomCoord; // Coordenada da sala inicial
    private Vector3 _gridOffset;
    private Camera _mainCamera;
    private bool _isTransitioning = false;

    public event Action<Vector2Int> OnRoomChanged;

    protected override void Awake()
    {
        base.Awake();
        _mainCamera = Camera.main;
        _gridOffset = new Vector3(
            -(gridSize.x - 1) * roomWidth / 2f,
            -(gridSize.y - 1) * roomHeight / 2f,
            0
        );

        // Define aleatoriamente a coordenada da sala inicial
        _initialRoomCoord = new Vector2Int(UnityEngine.Random.Range(0, gridSize.x), UnityEngine.Random.Range(0, gridSize.y));

        GenerateGrid();

        // Define a sala atual como a sala inicial
        SetCurrentRoom(_initialRoomCoord);

        // coloca o player no centro da sala inicial
        if (Player.Instance != null)
        {
            Player.Instance.transform.position = GetRoomWorldPosition(_initialRoomCoord);
            Debug.Log($"Player posicionado no centro da sala inicial {_initialRoomCoord}");
        }
    }

    void GenerateGrid()
    {
        // calcula a soma total das chances definidas das salas
        float totalChance = 0f;
        foreach (RoomOption option in roomOptions)
        {
            totalChance += option.spawnChance;
        }
        // Instancia as salas de acordo com a coordenada
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                Vector3 roomPosition = new Vector3(x * roomWidth, y * roomHeight, 0) + _gridOffset;
                GameObject roomObj = null;
                if (coord == _initialRoomCoord)
                {
                    // Instancia a sala inicial
                    roomObj = Instantiate(initialRoomPrefab, roomPosition, Quaternion.identity);
                    Debug.Log($"[GameManager] Sala inicial criada em {coord}.");
                }
                else
                {
                    // Seleção ponderada para escolher a opção de sala
                    float randomValue = UnityEngine.Random.Range(0f, totalChance);
                    float cumulative = 0f;
                    foreach (RoomOption option in roomOptions)
                    {
                        cumulative += option.spawnChance;
                        if (randomValue <= cumulative)
                        {
                            roomObj = Instantiate(option.roomPrefab, roomPosition, Quaternion.identity);
                            Debug.Log($"[GameManager] Sala '{option.roomName}' criada em {coord}.");
                            break;
                        }
                    }
                    // Fallback caso nenhuma opção seja selecionada (não deve ocorrer se as chances forem maiores que zero)
                    if (roomObj == null)
                    {
                        roomObj = Instantiate(roomOptions[0].roomPrefab, roomPosition, Quaternion.identity);
                        Debug.Log($"[GameManager] Fallback: Sala '{roomOptions[0].roomName}' criada em {coord}.");
                    }
                }
                Room room = roomObj.GetComponent<Room>();
                _rooms.Add(coord, room);
            }
        }
        // Inicializa cada sala (todas já estão instanciadas)
        foreach (KeyValuePair<Vector2Int, Room> pair in _rooms)
        {
            pair.Value.Initialize(pair.Key, roomWidth, roomHeight);
            Debug.Log($"[GameManager] Sala criada em {pair.Key}.");
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
