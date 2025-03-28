using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : Singleton<GameManager>
{
    #region Grid Settings
    [Header("Grid Parameters")]
    [SerializeField]
    private Vector2Int[] _gridPresets = {
        new Vector2Int(7, 6),  // Opções de tamanho do grid (X,Y)
        new Vector2Int(6, 5),
        new Vector2Int(5, 4),
        new Vector2Int(4, 4),
        new Vector2Int(4, 3)
    };

    public float roomWidth = 7f;       // Largura de cada sala
    public float roomHeight = 3.93f;   // Altura de cada sala
    public Vector2Int GridSize { get; private set; } // Tamanho escolhido do grid
    private Vector3 _gridOffset;       // Ajuste para centralizar o grid na tela
    #endregion

    #region Room Settings
    [Header("Room Configuration")]
    public GameObject initialRoomPrefab;   // Sala inicial obrigatória
    public List<RoomOption> roomOptions;   // Tipos de salas que podem ser geradas

    private Dictionary<Vector2Int, Room> _rooms = new Dictionary<Vector2Int, Room>(); // Mapa de salas
    private Vector2Int _currentRoomCoord;  // Coordenada da sala atual
    private Vector2Int _initialRoomCoord;  // Coordenada da sala inicial
    #endregion

    #region Door System
    [Header("Door Management")]
    private Dictionary<Vector2Int, HashSet<DoorDirection>> _doors = new Dictionary<Vector2Int, HashSet<DoorDirection>>(); // Portas existentes
    #endregion

    #region Player & Camera
    [Header("Player Settings")]
    public GameObject playerPrefab;    // Prefab do jogador
    private Camera _mainCamera;        // Referência da câmera principal
    private bool _isTransitioning = false; // Impede transições simultâneas
    public event Action<Vector2Int> OnRoomChanged; // Evento ao mudar de sala
    #endregion

    // Classe para configurar salas no Inspector
    [Serializable]
    public class RoomOption
    {
        public string roomName;     // Nome para identificação
        public GameObject roomPrefab; // Prefab da sala
        [Range(0, 100)] public float spawnChance; // Chance de aparecer (%)
    }

    // Inicialização quando o jogo começa
    protected override void Awake()
    {
        base.Awake(); // Garante a instância única
        InitializeGame();
    }

    // Configura todos os sistemas iniciais
    private void InitializeGame()
    {
        _mainCamera = Camera.main; // Pega a câmera principal
        SetupGrid();       // Cria o grid de salas
        GenerateWorld();   // Gera as salas do mapa
        InstantiatePlayer(); // Cria o jogador
    }

    // Escolhe um tamanho de grid aleatório
    private void SetupGrid()
    {
        // Sorteia um tamanho da lista de presets
        GridSize = _gridPresets[UnityEngine.Random.Range(0, _gridPresets.Length)];

        // Calcula offset para centralizar o grid
        _gridOffset = new Vector3(
            -(GridSize.x * roomWidth) / 2f + roomWidth / 2f,
            -(GridSize.y * roomHeight) / 2f + roomHeight / 2f,
            0
        );
    }

    // Gera todas as salas do mapa
    private void GenerateWorld()
    {
        // Escolhe coordenada aleatória para sala inicial
        _initialRoomCoord = new Vector2Int(
            UnityEngine.Random.Range(0, GridSize.x),
            UnityEngine.Random.Range(0, GridSize.y)
        );

        float totalChance = CalculateTotalSpawnChance(); // Soma todas as chances

        // Cria salas em todas as coordenadas do grid
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                CreateRoom(coord, totalChance); // Instancia cada sala
            }
        }
        SetCurrentRoom(_initialRoomCoord); // Define sala inicial como atual
    }

    // Cria uma sala em posição específica
    private void CreateRoom(Vector2Int coord, float totalChance)
    {
        // Calcula posição no mundo
        Vector3 position = new Vector3(
            coord.x * roomWidth,
            coord.y * roomHeight,
            0
        ) + _gridOffset;

        // Escolhe prefab: sala inicial ou aleatória
        GameObject prefab = (coord == _initialRoomCoord) ?
            initialRoomPrefab :
            SelectRandomRoomPrefab(totalChance);

        // Instancia e configura a sala
        Room room = Instantiate(prefab, position, Quaternion.identity).GetComponent<Room>();
        _rooms.Add(coord, room);
        room.Initialize(coord, roomWidth, roomHeight); // Inicializa a sala
    }

    // Seleciona sala aleatória baseada nas chances
    private GameObject SelectRandomRoomPrefab(float totalChance)
    {
        float randomValue = UnityEngine.Random.Range(0f, totalChance);
        float cumulative = 0f;

        // Percorre opções até achar a escolhida
        foreach (RoomOption option in roomOptions)
        {
            cumulative += option.spawnChance;
            if (randomValue <= cumulative)
                return option.roomPrefab;
        }
        return roomOptions[0].roomPrefab; // Fallback
    }

    // Soma todas as chances de spawn
    private float CalculateTotalSpawnChance()
    {
        float total = 0f;
        foreach (RoomOption option in roomOptions)
            total += option.spawnChance;
        return total;
    }

    // Cria o jogador na cena
    private void InstantiatePlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab não definido no GameManager!");
            return;
        }

        // Posiciona jogador no centro da câmera
        Vector3 spawnPos = _mainCamera.transform.position;
        spawnPos.z = 0; // Garante Z=0 para 2D
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    #region Public Methods
    // Registra uma porta no sistema
    public void RegisterDoor(Vector2Int roomCoord, DoorDirection direction)
    {
        if (!_doors.ContainsKey(roomCoord))
            _doors[roomCoord] = new HashSet<DoorDirection>();

        _doors[roomCoord].Add(direction);
    }

    // Verifica se uma porta existe
    public bool IsDoorAccessible(Vector2Int roomCoord, DoorDirection direction)
    {
        return _doors.ContainsKey(roomCoord) && _doors[roomCoord].Contains(direction);
    }

    // Converte coordenada do grid para posição no mundo
    public Vector3 GetRoomWorldPosition(Vector2Int coord)
    {
        return new Vector3(
            coord.x * roomWidth,
            coord.y * roomHeight,
            0
        ) + _gridOffset;
    }

    public Room GetRoom(Vector2Int coord) => _rooms.ContainsKey(coord) ? _rooms[coord] : null;
    public Vector2Int GetCurrentRoomCoord() => _currentRoomCoord;
    #endregion

    #region Room Transition
    // Atualiza sala atual e gerencia portas
    public void SetCurrentRoom(Vector2Int coord)
    {
        _currentRoomCoord = coord;
        OnRoomChanged?.Invoke(coord); // Dispara evento

        // Desativa portas em todas salas
        foreach (var room in _rooms.Values)
            room.SetActiveDoors(false);

        // Ativa portas apenas na sala atual
        if (_rooms.TryGetValue(coord, out Room currentRoom))
            currentRoom.SetActiveDoors(true);

        UpdateCameraPosition(); // Move a câmera
    }

    // Posiciona câmera no slot da sala atual
    private void UpdateCameraPosition()
    {
        if (_rooms.TryGetValue(_currentRoomCoord, out Room room) && room.cameraSlot != null)
            _mainCamera.transform.position = room.cameraSlot.position + new Vector3(0, 0, -10); // Mantém offset Z da câmera
    }

    // Tenta mover o jogador através de uma porta
    public void TryMoveThroughDoor(DoorDirection direction, float moveDistance)
    {
        Vector2Int dirVector = direction.ToVector();
        StartCoroutine(TransitionThroughDoor(dirVector, moveDistance));
    }

    // Animação de transição entre salas
    private IEnumerator TransitionThroughDoor(Vector2Int direction, float moveDistance)
    {
        if (_isTransitioning) yield break; // Evita transições simultâneas
        _isTransitioning = true;

        Vector2Int newCoord = _currentRoomCoord + direction;
        if (IsDoorAccessible(_currentRoomCoord, direction.ToDoorDirection()))
        {
            Collider2D playerCollider = Player.Instance.GetComponent<Collider2D>();
            playerCollider.enabled = false; // Previne colisões durante movimento

            // Calcula posições inicial e final
            Vector3 startPos = Player.Instance.transform.position;
            Vector3 targetPos = startPos + (Vector3)(Vector2)direction * moveDistance;

            yield return SmoothTransition(playerCollider, startPos, targetPos); // Animação
            SetCurrentRoom(newCoord); // Atualiza sala atual
        }
        _isTransitioning = false;
    }

    // Movimento suave do jogador
    private IEnumerator SmoothTransition(Collider2D collider, Vector3 start, Vector3 target)
    {
        float duration = 0.5f; // Tempo da animação
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Interpola posição gradualmente
            Player.Instance.transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Player.Instance.transform.position = target; // Posição final exata
        collider.enabled = true; // Reativa colisões
    }
    #endregion
}