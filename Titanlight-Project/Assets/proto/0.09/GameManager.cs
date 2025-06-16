using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using Player.StateMachine; // ajuste conforme seu namespace de PlayerManager, PlayerStateMachine

public class GameManager : Singleton<GameManager>
{
    #region Input Configuration
    [Header("Input Keys")]
    [Tooltip("Tecla para encerrar a run (pressionar e segurar)")]
    public KeyCode endRunKey = KeyCode.LeftShift;
    #endregion

    #region Auxiliar: Salvamento
    [Serializable]
    public class PlayerSaveData
    {
        public int savedMoney;
        public int savedReputation;
        public string lastSaveTime; // opcional
        // Você pode estender com playerName, achievements, inventory etc.
    }
    private string saveFileName = "savegame.json";
    #endregion

    #region Grid e Geração de Mapa
    [Header("Configuração do Grid")]
    [SerializeField]
    private Vector2Int[] _gridPresets = {
        new Vector2Int(7, 6),
        new Vector2Int(6, 5),
        new Vector2Int(5, 4),
        new Vector2Int(4, 4),
        new Vector2Int(4, 3)
    };
    public Vector2Int GridSize { get; private set; }

    [Header("Dimensões da Sala")]
    public float roomWidth = 7f;
    public float roomHeight = 3.93f;

    private Vector3 _gridOffset;

    [Header("Salas")]
    public GameObject initialRoomPrefab;
    [Serializable]
    public class RoomOption { public string roomName; public GameObject roomPrefab; [Range(0, 100)] public float spawnChance; }
    public List<RoomOption> roomOptions = new List<RoomOption>();

    private Dictionary<Vector2Int, Room> _rooms = new Dictionary<Vector2Int, Room>();
    private Vector2Int _initialRoomCoord;
    private Vector2Int _currentRoomCoord;
    #endregion

    #region Portas
    private Dictionary<Vector2Int, HashSet<DoorDirection>> _doors = new Dictionary<Vector2Int, HashSet<DoorDirection>>();
    #endregion

    #region UI / HUD
    [Header("UI Inicial")]
    public GameObject customizationCanvas;  // UI de customização antes da run

    [Header("HUD dos Jogadores")]
    public GameObject player1Canvas;
    public GameObject player2Canvas;

    [Header("Itens Coletados")]
    public TextMeshProUGUI[] itemCountTexts; // index 0 = player1, 1 = player2

    [Header("Dinheiro e Reputação (compartilhados)")]
    public TextMeshProUGUI[] moneyTexts;
    public TextMeshProUGUI[] reputationTexts;

    private int _collectedItems = 0;
    public int ScriptableObjectCount => _collectedItems;

    [Header("Leilão")]
    public Transform slotAuction;
    public GameObject auctionCanvas;

    [Header("Moonbox Slot")]
    [Tooltip("Transform que indica onde a câmera deve ir após o leilão, antes de nova run.")]
    public Transform slotMoonbox;
    #endregion

    #region Câmera
    private Camera _mainCamera;
    #endregion

    #region Controles de Tempo
    private float _shiftHoldTimer = 0f;
    private const float SHIFT_HOLD_DURATION = 2f;
    #endregion

    #region Contador de Jogadores Vivos
    private int _remainingPlayers = 0;
    private bool _isRunEnding = false;
    #endregion

    #region Singleton & Inicialização
    protected override void Awake()
    {
        base.Awake();

        // Se quiser persistir entre cenas, mas se tudo está na mesma cena, pode não ser necessário.
        DontDestroyOnLoad(gameObject);

        _mainCamera = Camera.main;

        // Logs de salvamento
        Debug.Log($"[GameManager] PersistentDataPath: {Application.persistentDataPath}");
        Debug.Log($"[GameManager] Save file: {SavePath}");

        // Verifica referências de UI para evitar NullReferenceException
        if (customizationCanvas == null) Debug.LogWarning("[GameManager] customizationCanvas não atribuído no Inspector!");
        if (player1Canvas == null) Debug.LogWarning("[GameManager] player1Canvas não atribuído no Inspector!");
        if (player2Canvas == null) Debug.LogWarning("[GameManager] player2Canvas não atribuído no Inspector!");
        if (auctionCanvas == null) Debug.LogWarning("[GameManager] auctionCanvas não atribuído no Inspector!");
        if (slotAuction == null) Debug.LogWarning("[GameManager] slotAuction não atribuído no Inspector!");
        if (slotMoonbox == null) Debug.LogWarning("[GameManager] slotMoonbox não atribuído no Inspector!");
        if (itemCountTexts == null) Debug.LogWarning("[GameManager] itemCountTexts array não atribuído no Inspector!");
        else
        {
            for (int i = 0; i < itemCountTexts.Length; i++)
                if (itemCountTexts[i] == null)
                    Debug.LogWarning($"[GameManager] itemCountTexts[{i}] não atribuído no Inspector!");
        }
        if (moneyTexts == null) Debug.LogWarning("[GameManager] moneyTexts array não atribuído no Inspector!");
        else
        {
            for (int i = 0; i < moneyTexts.Length; i++)
                if (moneyTexts[i] == null)
                    Debug.LogWarning($"[GameManager] moneyTexts[{i}] não atribuído no Inspector!");
        }
        if (reputationTexts == null) Debug.LogWarning("[GameManager] reputationTexts array não atribuído no Inspector!");
        else
        {
            for (int i = 0; i < reputationTexts.Length; i++)
                if (reputationTexts[i] == null)
                    Debug.LogWarning($"[GameManager] reputationTexts[{i}] não atribuído no Inspector!");
        }

        // Carrega dados persistentes
        LoadGame();

        // UI inicial antes de run
        if (customizationCanvas != null) customizationCanvas.SetActive(true);
        if (player1Canvas != null) player1Canvas.SetActive(false);
        if (player2Canvas != null) player2Canvas.SetActive(false);
        if (auctionCanvas != null) auctionCanvas.SetActive(false);

        if (itemCountTexts != null)
        {
            for (int i = 0; i < itemCountTexts.Length; i++)
                if (itemCountTexts[i] != null)
                    itemCountTexts[i].text = "0";
        }

        UpdatePersistentStatsUI();
    }
    #endregion

    #region Cleanup de Run
    /// <summary>
    /// Limpa todo estado residual da run anterior: salas, portas, jogadores, etc.
    /// Deve ser chamado antes de StartRun.
    /// </summary>
    private void CleanupPreviousRun()
    {
        Debug.Log("[GameManager] CleanupPreviousRun: limpando salas e estado de run anterior.");

        // Destrói salas restantes
        foreach (var r in _rooms.Values)
        {
            if (r != null)
                Destroy(r.gameObject);
        }
        _rooms.Clear();
        _doors.Clear();

        // Destrói jogadores
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.DestroyAllPlayers();

        // Reset flags e contadores
        _isRunEnding = false;
        _shiftHoldTimer = 0f;
        _collectedItems = 0;

        // Opcional: recapturar câmera
        if (_mainCamera == null) _mainCamera = Camera.main;
    }
    #endregion

    #region StartRun
    /// <summary>
    /// Inicia a run: limpa estado anterior, gera novo mapa, spawna player e ajusta câmera.
    /// Deve ser chamado quando o jogador escolher “iniciar nova run” (por ex. em UI de moonbox ou botão).
    /// </summary>
    public void StartRun()
    {
        Debug.Log("[GameManager] StartRun chamado");

        // Limpeza
        CleanupPreviousRun();

        // UI: esconde customização, mostra HUD de players
        if (customizationCanvas != null) customizationCanvas.SetActive(false);
        if (player1Canvas != null) player1Canvas.SetActive(true);
        if (player2Canvas != null) player2Canvas.SetActive(true);
        if (auctionCanvas != null) auctionCanvas.SetActive(false);

        // Inicializa contador de jogadores vivos
        int inicialCount = PlayerManager.Instance != null && PlayerManager.Instance.isMultiplayer ? 2 : 1;
        _remainingPlayers = inicialCount;
        Debug.Log($"[GameManager] Iniciando run com {_remainingPlayers} jogador(es).");

        // Geração do mundo
        SetupGrid();
        GenerateWorld();
        PairDoors();
        SetCurrentRoom(_initialRoomCoord);

        // Spawn players
        Room startRoom = GetRoom(_initialRoomCoord);
        if (startRoom != null)
        {
            Vector3 spawnPos = startRoom.GetPlayerSpawnPoint();
            spawnPos.z = 0f;
            if (PlayerManager.Instance != null)
                PlayerManager.Instance.SpawnPlayer(spawnPos);
            else
                Debug.LogWarning("[GameManager] StartRun: PlayerManager.Instance é null, não spawna player.");
        }
        else
        {
            Debug.LogWarning($"[GameManager] StartRun: startRoom é null para coord {_initialRoomCoord}");
        }

        // Atualiza UI de itens e stats
        if (itemCountTexts != null)
        {
            for (int i = 0; i < itemCountTexts.Length; i++)
                if (itemCountTexts[i] != null)
                    itemCountTexts[i].text = "0";
        }
        UpdatePersistentStatsUI();
    }
    #endregion

    #region Update (Shift para encerrar)
    private void Update()
    {
        if (_isRunEnding) return;

        if (Input.GetKey(endRunKey))
        {
            _shiftHoldTimer += Time.deltaTime;
            if (_shiftHoldTimer >= SHIFT_HOLD_DURATION)
            {
                _shiftHoldTimer = 0f;
                _isRunEnding = true;
                Debug.Log("[GameManager] Run encerrada manualmente pelo jogador.");
                EndRunAndAuction();
            }
        }
        else
        {
            _shiftHoldTimer = 0f;
        }
    }
    #endregion

    #region Grid & Salas
    private void SetupGrid()
    {
        if (_gridPresets == null || _gridPresets.Length == 0)
        {
            Debug.LogWarning("[GameManager] SetupGrid: _gridPresets vazio, usando padrão (5x5).");
            GridSize = new Vector2Int(5, 5);
        }
        else
        {
            GridSize = _gridPresets[UnityEngine.Random.Range(0, _gridPresets.Length)];
        }
        _gridOffset = new Vector3(
            -(GridSize.x * roomWidth) / 2f + roomWidth / 2f,
            -(GridSize.y * roomHeight) / 2f + roomHeight / 2f,
            0f
        );
        Debug.Log($"[GameManager] SetupGrid: GridSize={GridSize}, Offset={_gridOffset}");
    }

    private void GenerateWorld()
    {
        _initialRoomCoord = new Vector2Int(
            UnityEngine.Random.Range(0, GridSize.x),
            UnityEngine.Random.Range(0, GridSize.y)
        );
        Debug.Log($"[GameManager] GenerateWorld: initialRoomCoord={_initialRoomCoord}");

        float totalChance = 0f;
        if (roomOptions != null)
        {
            foreach (var o in roomOptions) totalChance += o.spawnChance;
        }
        else
        {
            Debug.LogWarning("[GameManager] GenerateWorld: roomOptions é null ou vazio!");
        }

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                CreateRoom(new Vector2Int(x, y), totalChance);
            }
        }
    }

    private void CreateRoom(Vector2Int coord, float totalChance)
    {
        Vector3 pos = new Vector3(coord.x * roomWidth, coord.y * roomHeight, 0f) + _gridOffset;
        GameObject prefab = (coord == _initialRoomCoord) ? initialRoomPrefab : SelectRandomRoomPrefab(totalChance);
        if (prefab == null)
        {
            Debug.LogWarning($"[GameManager] CreateRoom: prefab null para coord {coord}");
            return;
        }
        GameObject go = Instantiate(prefab, pos, Quaternion.identity);
        Room room = go.GetComponent<Room>();
        if (room == null)
        {
            Debug.LogWarning($"[GameManager] CreateRoom: prefab em coord {coord} não tem componente Room.");
            return;
        }
        room.Initialize(coord, roomWidth, roomHeight);
        _rooms.Add(coord, room);
    }

    private GameObject SelectRandomRoomPrefab(float totalChance)
    {
        if (roomOptions == null || roomOptions.Count == 0)
        {
            Debug.LogWarning("[GameManager] SelectRandomRoomPrefab: roomOptions vazio, retornando null.");
            return null;
        }
        float rnd = UnityEngine.Random.Range(0f, totalChance);
        float cum = 0f;
        foreach (var opt in roomOptions)
        {
            cum += opt.spawnChance;
            if (rnd <= cum)
            {
                if (opt.roomPrefab == null)
                    Debug.LogWarning($"[GameManager] SelectRandomRoomPrefab: roomPrefab null para opção {opt.roomName}");
                return opt.roomPrefab;
            }
        }
        // fallback
        if (roomOptions[0].roomPrefab == null)
            Debug.LogWarning("[GameManager] SelectRandomRoomPrefab: fallback roomPrefab null.");
        return roomOptions[0].roomPrefab;
    }
    #endregion

    #region Portas
    public void RegisterDoor(Vector2Int coord, DoorDirection dir)
    {
        if (!_doors.ContainsKey(coord))
            _doors[coord] = new HashSet<DoorDirection>();
        _doors[coord].Add(dir);
    }

    private void PairDoors()
    {
        foreach (var kv in _rooms)
        {
            var coord = kv.Key;
            var room = kv.Value;
            TryPair(room, coord, DoorDirection.Up, Vector2Int.up, DoorDirection.Down);
            TryPair(room, coord, DoorDirection.Right, Vector2Int.right, DoorDirection.Left);
            TryPair(room, coord, DoorDirection.Down, Vector2Int.down, DoorDirection.Up);
            TryPair(room, coord, DoorDirection.Left, Vector2Int.left, DoorDirection.Right);
        }
    }

    private void TryPair(Room room, Vector2Int coord, DoorDirection dir, Vector2Int off, DoorDirection opp)
    {
        var d = room.GetDoorTrigger(dir);
        var neighborRoom = GetRoom(coord + off);
        if (d == null || neighborRoom == null) return;
        var n = neighborRoom.GetDoorTrigger(opp);
        if (n == null) return;
        d.pairedDoor = n;
        n.pairedDoor = d;
    }

    public bool IsDoorAccessible(Vector2Int coord, DoorDirection dir)
        => _doors.ContainsKey(coord) && _doors[coord].Contains(dir);
    #endregion

    #region Sala Atual & Câmera
    public Vector2Int GetCurrentRoomCoord() => _currentRoomCoord;
    public Room GetRoom(Vector2Int coord) => _rooms.TryGetValue(coord, out var r) ? r : null;

    public void SetCurrentRoom(Vector2Int coord)
    {
        _currentRoomCoord = coord;
        if (_rooms.TryGetValue(coord, out var room))
        {
            room.SetActiveDoors(true);
            if (_mainCamera != null && room.cameraSlot != null)
                _mainCamera.transform.position = room.cameraSlot.position + Vector3.back * 10f;
        }
        OnRoomChanged?.Invoke(coord);
    }

    public event Action<Vector2Int> OnRoomChanged;
    #endregion

    #region Notificação de Morte de Jogador
    public void NotifyPlayerDied()
    {
        if (_isRunEnding) return;

        _remainingPlayers--;
        Debug.Log($"[GameManager] Um player morreu. Jogadores restantes: {_remainingPlayers}");

        if (_remainingPlayers <= 0)
        {
            _isRunEnding = true;
            Debug.Log("[GameManager] Todos os jogadores morreram. Encerrando run.");
            EndRunAndAuction();
        }
    }
    #endregion

    #region EndRun & Auction
    public void EndRunAndAuction()
    {
        if (!_isRunEnding)
        {
            _isRunEnding = true;
            Debug.Log("[GameManager] EndRunAndAuction chamado diretamente.");
        }
        // Destrói e limpa salas
        foreach (var r in _rooms.Values)
            if (r != null)
                Destroy(r.gameObject);
        _rooms.Clear();
        _doors.Clear();

        // Destrói jogadores
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.DestroyAllPlayers();

        // Move câmera para slot de leilão e ativa UI de leilão
        if (auctionCanvas != null) auctionCanvas.SetActive(true);
        if (_mainCamera != null && slotAuction != null)
            _mainCamera.transform.position = slotAuction.position + Vector3.back * 10f;

        // Mostra resumo de run
        FindObjectOfType<RunSummary>()?.ShowSummary();

        // Salva progresso
        SaveGame();
    }

    public void CompleteAuction()
    {
        // Fechar UI de leilão
        if (auctionCanvas != null) auctionCanvas.SetActive(false);

        // Move câmera para slotMoonbox
        if (_mainCamera != null && slotMoonbox != null)
            _mainCamera.transform.position = slotMoonbox.position + Vector3.back * 10f;

        // Ativar UI de customização para próxima run
        if (customizationCanvas != null) customizationCanvas.SetActive(true);

        UpdatePersistentStatsUI();

        // Agora a UI de customização pode ter botão que chama StartRun()
        SaveGame();
    }
    #endregion

    #region UI Atualização
    public void UpdatePersistentStatsUI()
    {
        int money = PlayerRuntimeData.GetMoney();
        int rep = PlayerRuntimeData.GetReputation();

        Debug.Log($"[GameManager] Atualizando UI de stats: Money={money}, Reputation={rep}");

        string m = money.ToString();
        string r = rep.ToString();
        if (moneyTexts != null)
        {
            for (int i = 0; i < moneyTexts.Length; i++)
            {
                if (moneyTexts[i] != null)
                    moneyTexts[i].text = m;
                else
                    Debug.LogWarning($"[GameManager] moneyTexts[{i}] é null em UpdatePersistentStatsUI.");
            }
        }
        if (reputationTexts != null)
        {
            for (int i = 0; i < reputationTexts.Length; i++)
            {
                if (reputationTexts[i] != null)
                    reputationTexts[i].text = r;
                else
                    Debug.LogWarning($"[GameManager] reputationTexts[{i}] é null em UpdatePersistentStatsUI.");
            }
        }
    }

    public void RegisterScriptableObject(int amount)
    {
        _collectedItems += amount;
        string count = _collectedItems.ToString();
        if (itemCountTexts != null)
        {
            for (int i = 0; i < itemCountTexts.Length; i++)
            {
                if (itemCountTexts[i] != null)
                    itemCountTexts[i].text = count;
            }
        }
    }
    #endregion

    #region Save/Load
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public void SaveGame()
    {
        try
        {
            var data = new PlayerSaveData
            {
                savedMoney = PlayerRuntimeData.GetMoney(),
                savedReputation = PlayerRuntimeData.GetReputation(),
                lastSaveTime = DateTime.UtcNow.ToString("o"),
            };
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[GameManager] Game Saved to {SavePath} | Money: {data.savedMoney} | Rep: {data.savedReputation}");
            Debug.Log($"[GameManager] Conteúdo salvo (JSON): {json}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameManager] Erro ao salvar jogo em {SavePath}: {ex}");
        }
    }

    public void LoadGame()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Debug.Log($"[GameManager] LoadGame: encontrado arquivo. Conteúdo JSON: {json}");
                var data = JsonUtility.FromJson<PlayerSaveData>(json);
                Debug.Log($"[GameManager] Valores carregados: Money={data.savedMoney}, Rep={data.savedReputation}");
                PlayerRuntimeData.Initialize(data.savedMoney, data.savedReputation);
            }
            else
            {
                Debug.Log($"[GameManager] LoadGame: nenhum arquivo encontrado em {SavePath}. Iniciando novo jogo com valores padrão.");
                PlayerRuntimeData.Reset();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameManager] Erro ao carregar jogo de {SavePath}: {ex}. Usando valores padrão.");
            PlayerRuntimeData.Reset();
        }
    }
    #endregion

    #region Eventos e outros métodos
   
    #endregion
}
