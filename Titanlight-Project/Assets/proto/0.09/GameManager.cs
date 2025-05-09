using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Player.StateMachine;
using TMPro;
using System.IO;

public class GameManager : Singleton<GameManager>
{
    // Classe para dados de salvamento
    [Serializable]
    public class PlayerSaveData
    {
        public int savedMoney;
        public int savedReputation;
    }

    // Configuração do grid de salas
    [Header("Configuração do Grid")]
    [SerializeField]
    private Vector2Int[] _gridPresets = {
        new Vector2Int(7,6),
        new Vector2Int(6,5),
        new Vector2Int(5,4),
        new Vector2Int(4,4),
        new Vector2Int(4,3)
    };

    // Dimensões das salas
    [Header("Dimensões da Sala")]
    public float roomWidth = 7f;
    public float roomHeight = 3.93f;
    public Vector2Int GridSize { get; private set; }
    private Vector3 _gridOffset;

    // Sistema de geração de salas
    [Header("Configuração de Salas")]
    public GameObject initialRoomPrefab;
    public List<RoomOption> roomOptions;
    private Dictionary<Vector2Int, Room> _rooms = new Dictionary<Vector2Int, Room>();
    private Vector2Int _initialRoomCoord;
    private Vector2Int _currentRoomCoord;

    // Gerenciamento de portas
    [Header("Gerenciamento de Portas")]
    private Dictionary<Vector2Int, HashSet<DoorDirection>> _doors =
        new Dictionary<Vector2Int, HashSet<DoorDirection>>();

    // Configurações do jogador
    [Header("Configurações do Jogador")]
    public GameObject playerPrefab;
    private GameObject _currentPlayer;
    private Camera _mainCamera;
    private bool _isTransitioning = false;
    public event Action<Vector2Int> OnRoomChanged;

    // Opções de salas serializáveis
    [Serializable]
    public class RoomOption
    {
        public string roomName;
        public GameObject roomPrefab;
        [Range(0, 100)] public float spawnChance;
    }

    // Elementos de UI
    [Header("Elementos de UI")]
    public GameObject customizationCanvas;
    public Transform slotMoonBox;
    public Transform slotStartCamera;

    [Header("HUD do Jogador")]
    public GameObject playerStatusCanvas;
    public GameObject playerOtherCanvas;
    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI reputationText;

    [Header("Configurações de Leilão")]
    public Transform slotAuction;
    public GameObject auctionCanvas;

    // Sistema de salvamento
    private string saveFileName = "savegame.json";
    private int _scriptableObjectCount = 0;
    public int ScriptableObjectCount => _scriptableObjectCount;

    // Controle de input
    private float _shiftHoldTimer = 0f;
    private const float SHIFT_HOLD_DURATION = 2f;

    // Inicialização do singleton
    protected override void Awake()
    {
        base.Awake(); // Chama a implementação base do Singleton
        _mainCamera = Camera.main; // Obtém referência da câmera
        LoadGame(); // Carrega dados salvos

        // Configuração inicial da UI
        customizationCanvas.SetActive(true);
        playerStatusCanvas.SetActive(false);
        playerOtherCanvas.SetActive(false);
        if (auctionCanvas != null) auctionCanvas.SetActive(false);

        UpdatePersistentStatsUI(); // Atualiza valores na UI
        if (itemCountText != null) itemCountText.text = "0";
        if (slotMoonBox != null)
            _mainCamera.transform.position = slotMoonBox.position + Vector3.back * 10f;

        // Registra evento de atualização de status
        PlayerRuntimeData.OnStatsChanged += UpdatePersistentStatsUI;
    }

    // Limpeza ao destruir
    private void OnDestroy()
    {
        PlayerRuntimeData.OnStatsChanged -= UpdatePersistentStatsUI;
    }

    // Inicia uma nova partida
    public void StartRun()
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        _scriptableObjectCount = 0; // Reseta contador de itens
        if (itemCountText != null) itemCountText.text = "0";

        // Configura UI
        customizationCanvas.SetActive(false);
        playerStatusCanvas.SetActive(true);
        playerOtherCanvas.SetActive(true);

        // Gera mundo
        SetupGrid();
        GenerateWorld();
        PairDoors();
        SetCurrentRoom(_initialRoomCoord);
        InstantiatePlayer();

        // Reseta timer se existir
        var timer = FindObjectOfType<CountdownTimer>();
        if (timer != null) timer.ResetTimer();
    }

    // Atualiza UI com valores persistentes
    public void UpdatePersistentStatsUI()
    {
        if (moneyText != null)
            moneyText.text = PlayerRuntimeData.GetMoney().ToString();

        if (reputationText != null)
            reputationText.text = PlayerRuntimeData.GetReputation().ToString();
    }

    // Configura o grid aleatório
    private void SetupGrid()
    {
        GridSize = _gridPresets[UnityEngine.Random.Range(0, _gridPresets.Length)];
        _gridOffset = new Vector3(
            -(GridSize.x * roomWidth) / 2f + roomWidth / 2f,
            -(GridSize.y * roomHeight) / 2f + roomHeight / 2f,
            0f);
    }

    // Gera o mundo proceduralmente
    private void GenerateWorld()
    {
        // Escolhe coordenada inicial aleatória
        _initialRoomCoord = new Vector2Int(
            UnityEngine.Random.Range(0, GridSize.x),
            UnityEngine.Random.Range(0, GridSize.y));

        float totalChance = CalculateTotalSpawnChance();

        // Cria todas as salas
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                CreateRoom(new Vector2Int(x, y), totalChance);
            }
        }
    }

    // Cria uma sala específica
    private void CreateRoom(Vector2Int coord, float totalChance)
    {
        Vector3 pos = new Vector3(coord.x * roomWidth, coord.y * roomHeight, 0f) + _gridOffset;
        GameObject prefab = (coord == _initialRoomCoord) ?
            initialRoomPrefab :
            SelectRandomRoomPrefab(totalChance);

        Room room = Instantiate(prefab, pos, Quaternion.identity).GetComponent<Room>();
        room.Initialize(coord, roomWidth, roomHeight);
        _rooms.Add(coord, room);
    }

    // Seleciona prefab aleatório baseado nas chances
    private GameObject SelectRandomRoomPrefab(float totalChance)
    {
        float rnd = UnityEngine.Random.Range(0f, totalChance);
        float cum = 0f;
        foreach (var opt in roomOptions)
        {
            cum += opt.spawnChance;
            if (rnd <= cum) return opt.roomPrefab;
        }
        return roomOptions[0].roomPrefab;
    }

    // Calcula total de chances para spawn
    private float CalculateTotalSpawnChance()
    {
        float total = 0f;
        foreach (var o in roomOptions) total += o.spawnChance;
        return total;
    }

    // Instancia o jogador na sala inicial
    private void InstantiatePlayer()
    {
        Room startRoom = GetRoom(_initialRoomCoord);
        if (startRoom == null) return;

        Vector3 spawnPos = startRoom.GetPlayerSpawnPoint();
        spawnPos.z = 0f;
        _currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        // Registra evento de morte
        Health health = _currentPlayer.GetComponent<Health>();
        if (health != null) health.onDeath.AddListener(OnPlayerDeath);
    }

    // Handler de morte do jogador
    private void OnPlayerDeath() => EndRunAndAuction();

    // Update verifica input de força de término
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            _shiftHoldTimer += Time.deltaTime;
            if (_shiftHoldTimer >= SHIFT_HOLD_DURATION)
            {
                EndRunAndAuction();
                _shiftHoldTimer = 0f;
            }
        }
        else _shiftHoldTimer = 0f;
    }

    // Finaliza a run e inicia leilão
    private void EndRunAndAuction()
    {
        if (_currentPlayer != null) Destroy(_currentPlayer);
        ExitRun();
        if (auctionCanvas != null) auctionCanvas.SetActive(true);
        if (slotAuction != null)
            _mainCamera.transform.position = slotAuction.position + Vector3.back * 10f;

        // Mostra resumo
        RunSummary summary = FindObjectOfType<RunSummary>();
        if (summary != null) summary.ShowSummary();
        SaveGame();
    }

    // Limpa estado da run
    private void ExitRun()
    {
        // Destroi salas
        foreach (var r in _rooms.Values)
            if (r != null) Destroy(r.gameObject);

        _rooms.Clear();
        _doors.Clear();

        // Desativa timer
        CountdownTimer timer = FindObjectOfType<CountdownTimer>();
        if (timer != null) timer.gameObject.SetActive(false);

        // Atualiza UI
        playerStatusCanvas.SetActive(false);
        playerOtherCanvas.SetActive(false);
    }

    // Finaliza o leilão
    public void CompleteAuction()
    {
        if (auctionCanvas != null) auctionCanvas.SetActive(false);
        if (slotMoonBox != null)
            _mainCamera.transform.position = slotMoonBox.position + Vector3.back * 10f;

        customizationCanvas.SetActive(true);
        SaveGame();
        UpdatePersistentStatsUI(); // Força atualização imediata
    }

    // Sistema de salvamento
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public void SaveGame()
    {
        PlayerSaveData data = new PlayerSaveData()
        {
            savedMoney = PlayerRuntimeData.GetMoney(),
            savedReputation = PlayerRuntimeData.GetReputation()
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game Saved | Money: {data.savedMoney} | Rep: {data.savedReputation}");
    }

    public void LoadGame()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

            PlayerRuntimeData.Initialize(
                data.savedMoney,
                data.savedReputation
            );
            Debug.Log($"Game Loaded | Money: {data.savedMoney} | Rep: {data.savedReputation}");
        }
        else
        {
            Debug.Log("New Game Started");
            PlayerRuntimeData.Reset();
        }
    }

    // Registra coleta de itens
    public void RegisterScriptableObject(int amount)
    {
        _scriptableObjectCount += amount;
        if (itemCountText != null)
            itemCountText.text = _scriptableObjectCount.ToString();
    }

    // Navegação entre salas
    public Vector2Int GetCurrentRoomCoord() => _currentRoomCoord;

    public void RegisterDoor(Vector2Int coord, DoorDirection dir)
    {
        if (!_doors.ContainsKey(coord))
            _doors[coord] = new HashSet<DoorDirection>();

        _doors[coord].Add(dir);
    }

    public void TryMoveThroughDoor(DoorDirection dir, float dist) =>
        StartCoroutine(TransitionThroughDoor(dir.ToVector(), dist));

    private IEnumerator TransitionThroughDoor(Vector2Int dir, float dist)
    {
        if (_isTransitioning) yield break;
        _isTransitioning = true;

        if (IsDoorAccessible(_currentRoomCoord, dir.ToDoorDirection()))
        {
            Vector2Int newCoord = _currentRoomCoord + dir;
            if (!_rooms.ContainsKey(newCoord))
            {
                _isTransitioning = false;
                yield break;
            }

            Collider2D col = PlayerStateMachine.Instance.GetComponent<Collider2D>();
            col.enabled = false;

            Vector3 start = PlayerStateMachine.Instance.transform.position;
            Vector3 offset = new Vector3(dir.x, dir.y, 0f) * dist;
            Vector3 target = start + offset;

            yield return SmoothTransition(col, start, target);
            SetCurrentRoom(newCoord);
        }
        _isTransitioning = false;
    }

    private IEnumerator SmoothTransition(Collider2D col, Vector3 a, Vector3 b)
    {
        float duration = 0.5f, t = 0f;
        while (t < duration)
        {
            PlayerStateMachine.Instance.transform.position =
                Vector3.Lerp(a, b, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        PlayerStateMachine.Instance.transform.position = b;
        col.enabled = true;
    }

    // Sistema de portas
    private void PairDoors()
    {
        foreach (var kv in _rooms)
        {
            Vector2Int coord = kv.Key;
            Room room = kv.Value;

            TryPair(room, coord, DoorDirection.Up, Vector2Int.up, DoorDirection.Down);
            TryPair(room, coord, DoorDirection.Right, Vector2Int.right, DoorDirection.Left);
            TryPair(room, coord, DoorDirection.Down, Vector2Int.down, DoorDirection.Up);
            TryPair(room, coord, DoorDirection.Left, Vector2Int.left, DoorDirection.Right);
        }
    }

    private void TryPair(Room room, Vector2Int coord, DoorDirection dir, Vector2Int off, DoorDirection opp)
    {
        var d = room.GetDoorTrigger(dir);
        var n = GetRoom(coord + off)?.GetDoorTrigger(opp);
        if (d != null && n != null)
        {
            d.pairedDoor = n;
            n.pairedDoor = d;
        }
    }

    public bool IsDoorAccessible(Vector2Int coord, DoorDirection dir) =>
        _doors.ContainsKey(coord) && _doors[coord].Contains(dir);

    // Controle de sala atual
    public void SetCurrentRoom(Vector2Int coord)
    {
        _currentRoomCoord = coord;
        OnRoomChanged?.Invoke(coord);

        foreach (var room in _rooms.Values)
            room.SetActiveDoors(false);

        if (_rooms.TryGetValue(coord, out var current))
            current.SetActiveDoors(true);

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (_rooms.TryGetValue(_currentRoomCoord, out var room) && room.cameraSlot != null)
            _mainCamera.transform.position = room.cameraSlot.position + Vector3.back * 10f;
    }

    public Room GetRoom(Vector2Int coord)
    {
        return _rooms.TryGetValue(coord, out var room) ? room : null;
    }

    // Navegação para menu
    public void GoToMainMenu() => SceneManager.LoadScene("MainMenu");
}