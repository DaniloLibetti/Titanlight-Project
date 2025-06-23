using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    #region Input Configuration
    [Header("Input Keys")]
    public KeyCode endRunKey = KeyCode.LeftShift;
    private const float SHIFT_HOLD_DURATION = 2f;
    private float _shiftHoldTimer = 0f;
    #endregion

    #region Auxiliar: Salvamento
    [Serializable]
    public class PlayerSaveData
    {
        public int savedMoney;
        public int savedReputation;
        public string lastSaveTime;
    }
    private string saveFileName = "savegame.json";
    #endregion

    #region Grid e Geração de Mapa
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

    #region Doors & Navigation
    private Dictionary<Vector2Int, HashSet<DoorDirection>> _doors = new Dictionary<Vector2Int, HashSet<DoorDirection>>();
    private Dictionary<Vector2Int, Dictionary<DoorDirection, DoorState>> _doorStates = new Dictionary<Vector2Int, Dictionary<DoorDirection, DoorState>>();

    public void RegisterDoor(Vector2Int coord, DoorDirection dir)
    {
        if (!_doors.ContainsKey(coord)) _doors[coord] = new HashSet<DoorDirection>();
        _doors[coord].Add(dir);
        if (!_doorStates.ContainsKey(coord)) _doorStates[coord] = new Dictionary<DoorDirection, DoorState>();
        if (!_doorStates[coord].ContainsKey(dir)) _doorStates[coord][dir] = new DoorState();
    }

    public DoorState GetDoorState(Vector2Int coord, DoorDirection dir)
    {
        if (_doorStates.TryGetValue(coord, out var dict) && dict.TryGetValue(dir, out var state)) return state;
        if (!_doorStates.ContainsKey(coord)) _doorStates[coord] = new Dictionary<DoorDirection, DoorState>();
        var newState = new DoorState();
        _doorStates[coord][dir] = newState;
        return newState;
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

    private void TryPair(Room room, Vector2Int coord, DoorDirection dir, Vector2Int offset, DoorDirection opposite)
    {
        var d = room.GetDoorTrigger(dir);
        var nr = GetRoom(coord + offset);
        if (d == null || nr == null) return;
        var nd = nr.GetDoorTrigger(opposite);
        if (nd == null) return;
        d.pairedDoor = nd;
        nd.pairedDoor = d;
        var state = GetDoorState(coord, dir);
        d.sharedState = state;
        nd.sharedState = state;
    }

    public bool IsDoorAccessible(Vector2Int coord, DoorDirection dir) => _doors.ContainsKey(coord) && _doors[coord].Contains(dir);
    public Room GetRoom(Vector2Int coord) { _rooms.TryGetValue(coord, out var room); return room; }

    public void SetCurrentRoom(Vector2Int coord, DoorDirection entryDirection)
    {
        _currentRoomCoord = coord;
        _currentRoomPirates.Clear();
        OnRoomChanged?.Invoke(coord);
        var room = GetRoom(coord);
        if (room != null)
        {
            if (Camera.main != null && room.cameraSlot != null)
                Camera.main.transform.position = room.cameraSlot.position + Vector3.back * 10f;
            var spawn = room.GetSpawnPointByDoorDirection(entryDirection);
            PlayerManager.Instance.MoveAllPlayers(spawn);
        }
    }
    #endregion

    #region UI / Ready / Run Flow
    [Header("UI Inicial (Customização/Moonbox)")]
    public GameObject customizationCanvas;
    public GameObject player1Canvas;
    public GameObject player2Canvas;
    [Header("Ready/Start UI")]
    public Button p1ReadyButton;
    public Animator p1ReadyAnimator;
    public Button p2ReadyButton;
    public Animator p2ReadyAnimator;
    public Button startButton;
    public TextMeshProUGUI startHintText;
    private bool player1Ready = false;
    private bool player2Ready = false;
    #endregion

    [Header("Leilão")]
    public GameObject auctionCanvas;
    public Transform slotAuction;
    public Transform slotMoonbox;
    public TextMeshProUGUI auctionValueText;
    public TextMeshProUGUI reputationValueText;

    [Header("Finalização de Run")]
    public GameObject runEndCanvas;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public Button victoryToAuctionButton;
    public Button defeatToMoonboxButton;
    public Button defeatToMainMenuButton;
    public Button defeatQuitButton;

    [Header("Pirate Spawn")]
    public GameObject piratePrefab;
    public float initialSpawnDelay = 5f;
    public float spawnInterval = 10f;
    public int maxPiratesPerRoom = 6;
    public int initialPirateGroupSize = 3;
    public CountdownTimer pirateTimer;
    private bool isMultiplayer;
    public bool IsMultiplayer => isMultiplayer;
    private bool _isRunEnding = false;
    private int _remainingPlayers;
    private bool _victoryEnding = false;
    private List<GameObject> _currentRoomPirates = new List<GameObject>();
    private Coroutine _pirateSpawnRoutine;
    public event Action<Vector2Int> OnRoomChanged;

    [Header("Run Summary")]
    public RunSummary runSummary;

    [Header("Player Settings")]
    [Tooltip("Player index (1 or 2)")]
    public int playerIndex = 1;

    [SerializeField] private GameObject healthBarObject1;
    [SerializeField] private GameObject healthBarObject2;

    public Vector2Int GetCurrentRoomCoord() => _currentRoomCoord;
    public int ScriptableObjectCount => _collectedItems;

    public void CompleteAuction()
    {
        auctionCanvas?.SetActive(false);
        customizationCanvas?.SetActive(true);

        // RESETA contagem de itens após leilão
        _collectedItems = 0;
        ResetStatsUI();

        // Reposiciona a câmera no moonbox após o leilão
        if (slotMoonbox != null && Camera.main != null)
        {
            Camera.main.transform.position = slotMoonbox.position + Vector3.back * 10f;
        }
    }

    #region [TIMER UI]
    [Header("Timer UI")]
    public GameObject timerCanvas;
    #endregion

    #region Singleton & Inicialização
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        isMultiplayer = PlayerPrefs.GetInt("IsMultiplayer", 0) == 1;
        SetupCustomizationUI();
        SetupReadyUI();
        SetupRunEndUI();

        if (pirateTimer == null)
        {
            Debug.LogWarning("[GameManager] pirateTimer NÃO atribuído no Inspector!");
        }
        else
        {
            pirateTimer.Invasion.RemoveAllListeners();
            pirateTimer.Invasion.AddListener(OnInvasionTimerEnded);
        }

        LoadGame();
        UpdatePersistentStatsUI();
        EnsurePlayerManagerExists();
    }

    private void EnsurePlayerManagerExists()
    {
        if (PlayerManager.Instance == null)
        {
            var pmObj = new GameObject("PlayerManager");
            pmObj.AddComponent<PlayerManager>();
        }
    }
    #endregion

    private void OnInvasionTimerEnded()
    {
        if (_pirateSpawnRoutine != null) StopCoroutine(_pirateSpawnRoutine);
        _pirateSpawnRoutine = StartCoroutine(PirateInvasionRoutine());
    }

    private IEnumerator PirateInvasionRoutine()
    {
        if (initialSpawnDelay > 0f) yield return new WaitForSeconds(initialSpawnDelay);

        while (!_isRunEnding)
        {
            if (_currentRoomPirates.Count < maxPiratesPerRoom)
            {
                SpawnPirateInCurrentRoom();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SetupCustomizationUI()
    {
        customizationCanvas?.SetActive(true);
        auctionCanvas?.SetActive(false);
        ResetStatsUI();
        if (pirateTimer != null) pirateTimer.gameObject.SetActive(false);
        if (timerCanvas != null) timerCanvas.SetActive(false);

        // Reposiciona a câmera no moonbox
        if (slotMoonbox != null && Camera.main != null)
        {
            Camera.main.transform.position = slotMoonbox.position + Vector3.back * 10f;
        }
    }

    private void SetupReadyUI()
    {
        player1Ready = player2Ready = false;
        p1ReadyAnimator?.SetBool("isReady", false);
        p2ReadyAnimator?.SetBool("isReady", false);

        if (p1ReadyButton != null) p1ReadyButton.onClick.AddListener(OnP1ReadyToggled);
        if (p2ReadyButton != null) p2ReadyButton.gameObject.SetActive(isMultiplayer);
        if (isMultiplayer && p2ReadyButton != null) p2ReadyButton.onClick.AddListener(OnP2ReadyToggled);
        if (player2Canvas != null) player2Canvas.SetActive(isMultiplayer);
        if (startButton != null) startButton.onClick.AddListener(OnStartPressed);
        if (startHintText != null) startHintText.text = "";

        UpdateStartButtonUI();
    }

    private void SetupRunEndUI()
    {
        if (runEndCanvas != null) runEndCanvas.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (victoryToAuctionButton != null) victoryToAuctionButton.onClick.AddListener(OnVictoryToAuction);
        if (defeatToMoonboxButton != null) defeatToMoonboxButton.onClick.AddListener(OnDefeatToMoonbox);
        if (defeatToMainMenuButton != null) defeatToMainMenuButton.onClick.AddListener(OnDefeatToMainMenu);
        if (defeatQuitButton != null) defeatQuitButton.onClick.AddListener(OnDefeatQuit);
    }

    #region Ready/Start Callbacks
    private void OnP1ReadyToggled() => SetPlayerReady(1, !player1Ready);
    private void OnP2ReadyToggled() => SetPlayerReady(2, !player2Ready);

    public void SetPlayerReady(int index, bool ready)
    {
        if (index == 1) player1Ready = ready;
        else if (index == 2) player2Ready = ready;

        if (p1ReadyAnimator != null) p1ReadyAnimator.SetBool("isReady", player1Ready);
        if (p2ReadyAnimator != null) p2ReadyAnimator.SetBool("isReady", player2Ready);

        UpdateStartButtonUI();
    }

    public void UpdateStartButtonUI()
    {
        bool canStart = player1Ready && (!isMultiplayer || player2Ready);
        if (startButton != null)
        {
            startButton.interactable = canStart;
            if (startButton.image != null)
                startButton.image.color = canStart ? Color.green : Color.red;
        }
        if (startHintText != null) startHintText.text = "";
    }

    private void OnStartPressed()
    {
        if (player1Ready && (!isMultiplayer || player2Ready))
        {
            BeginRun();
        }
        else
        {
            if (startHintText != null)
                startHintText.text = !player1Ready ? "Jogador 1 não está pronto" : "Jogador 2 não está pronto";
        }
    }
    #endregion

    #region Run Flow
    private void BeginRun() => StartRun();

    public void StartRun()
    {
        customizationCanvas?.SetActive(false);
        auctionCanvas?.SetActive(false);
        

        CleanupPreviousRun();
        SetupGrid();
        GenerateWorld();
        PairDoors();
        SetCurrentRoom(_initialRoomCoord, DoorDirection.Up);
        _remainingPlayers = IsMultiplayer ? 2 : 1;
        EnsurePlayerManagerExists();
        SpawnPlayers();
        ShowBar();
        if (timerCanvas != null) timerCanvas.SetActive(true);

        // RESETA valores da run
        _collectedItems = 0;
        ResetStatsUI();

        if (pirateTimer != null)
        {
            pirateTimer.gameObject.SetActive(true);
            pirateTimer.ResetTimer();
        }

        UpdatePersistentStatsUI();
        player1Ready = player2Ready = false;
        UpdateStartButtonUI();
    }

    private void Update()
    {
        if (_isRunEnding) return;

        // Verificação adicional de jogadores vivos
        if (_remainingPlayers <= 0)
        {
            _isRunEnding = true;
            EndRun();
            return;
        }

        if (Input.GetKey(endRunKey))
        {
            _shiftHoldTimer += Time.deltaTime;
            if (_shiftHoldTimer >= SHIFT_HOLD_DURATION)
            {
                _isRunEnding = true;
                _victoryEnding = true;
                EndRun();
            }
        }
        else if (_shiftHoldTimer > 0f)
        {
            _shiftHoldTimer = 0f;
        }
    }

    private void SpawnPirateInCurrentRoom()
    {
        var room = GetRoom(_currentRoomCoord);
        if (room == null || piratePrefab == null) return;

        var spawnPos = GetSafeSpawnPosition(room);
        var pirate = Instantiate(piratePrefab, spawnPos, Quaternion.identity);
        _currentRoomPirates.Add(pirate);

        var chase = pirate.GetComponent<ChasingEnemy>();
        if (chase != null) chase.ActivateInRoom(room.roomCollider);

        var health = pirate.GetComponent<Health>();
        if (health != null) health.onDeath.AddListener(() => OnPirateDeath(pirate));
    }

    private Vector3 GetSafeSpawnPosition(Room room)
    {
        var bounds = room.roomCollider.bounds;
        for (int i = 0; i < 50; i++)
        {
            var pos = new Vector3(
                UnityEngine.Random.Range(bounds.min.x + 1f, bounds.max.x - 1f),
                UnityEngine.Random.Range(bounds.min.y + 1f, bounds.max.y - 1f),
                0f);
            if (Physics2D.OverlapCircle(pos, 0.4f, LayerMask.GetMask("HoleFloor")) == null) return pos;
        }
        return bounds.center;
    }

    private void OnPirateDeath(GameObject pirate)
    {
        if (_currentRoomPirates.Contains(pirate))
        {
            _currentRoomPirates.Remove(pirate);
        }
    }
    #endregion

    #region Map Generation & Cleanup
    private void CleanupPreviousRun()
    {
        // Destruir todas as salas
        foreach (var r in _rooms.Values)
        {
            if (r != null && r.gameObject != null)
            {
                Destroy(r.gameObject);
            }
        }
        _rooms.Clear();

        // Resetar PlayerManager completamente
        PlayerManager.Instance?.DestroyAllPlayers(true);

        // Destruir piratas
        foreach (var p in _currentRoomPirates)
        {
            if (p != null)
            {
                Destroy(p);
            }
        }
        _currentRoomPirates.Clear();

        // Resetar estados da run
        _remainingPlayers = IsMultiplayer ? 2 : 1;
        _isRunEnding = false;
        _victoryEnding = false;
        _shiftHoldTimer = 0f;
        UpdateDoorRequirements();

        // Parar rotina de spawn
        if (_pirateSpawnRoutine != null)
        {
            StopCoroutine(_pirateSpawnRoutine);
            _pirateSpawnRoutine = null;
        }

        // Desativar timers
        if (pirateTimer != null) pirateTimer.gameObject.SetActive(false);
        if (timerCanvas != null) timerCanvas.SetActive(false);
    }

    private void SetupGrid()
    {
        GridSize = (_gridPresets.Length > 0)
            ? _gridPresets[UnityEngine.Random.Range(0, _gridPresets.Length)]
            : new Vector2Int(5, 5);
        _gridOffset = new Vector3(
            -(GridSize.x * roomWidth) / 2f + roomWidth / 2f,
            -(GridSize.y * roomHeight) / 2f + roomHeight / 2f,
            0f);
    }

    private void GenerateWorld()
    {
        float totalChance = 0f;
        foreach (var opt in roomOptions) totalChance += opt.spawnChance;
        _initialRoomCoord = new Vector2Int(
            UnityEngine.Random.Range(0, GridSize.x),
            UnityEngine.Random.Range(0, GridSize.y));

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                CreateRoom(new Vector2Int(x, y), totalChance);
    }

    private void CreateRoom(Vector2Int coord, float totalChance)
    {
        GameObject prefab = coord == _initialRoomCoord ?
            initialRoomPrefab :
            ChoosePrefab(totalChance);

        if (prefab == null) return;

        var pos = new Vector3(coord.x * roomWidth, coord.y * roomHeight, 0f) + _gridOffset;
        var go = Instantiate(prefab, pos, Quaternion.identity);
        var comp = go.GetComponent<Room>();
        if (comp != null)
        {
            comp.Initialize(coord, roomWidth, roomHeight);
            _rooms[coord] = comp;
        }
        else Destroy(go);
    }

    private GameObject ChoosePrefab(float total)
    {
        float r = UnityEngine.Random.Range(0f, total), sum = 0f;
        foreach (var opt in roomOptions)
        {
            sum += opt.spawnChance;
            if (r <= sum && opt.roomPrefab != null) return opt.roomPrefab;
        }
        return roomOptions.Count > 0 ? roomOptions[0].roomPrefab : null;
    }
    #endregion

    public void SpawnPlayers()
    {
        var room = GetRoom(_initialRoomCoord);
        var baseSpawn = room != null ?
            room.GetPlayerSpawnPoint() :
            Vector3.zero;

        PlayerManager.Instance.SpawnPlayers(baseSpawn);

        // Força estado inicial correto
        _remainingPlayers = IsMultiplayer ? 2 : 1;
        UpdateDoorRequirements();
    }

    public void RegisterPlayer(int playerIndex, bool isActive)
    {
        Debug.Log($"[GameManager] Player {playerIndex} registered (active: {isActive})");
    }

    public void MoveAllPlayers(Vector3 position)
    {
        PlayerManager.Instance.MoveAllPlayers(position);
    }

    // Sistema de portas adaptativo
    public void UpdateDoorRequirements()
    {
        bool singlePlayerAlive = (_remainingPlayers == 1);

        foreach (var room in _doorStates)
        {
            foreach (var door in room.Value)
            {
                door.Value.requiresTwoPlayers = !singlePlayerAlive;
            }
        }
    }

    public void NotifyPlayerDied()
    {
        if (_isRunEnding) return;

        _remainingPlayers--;
        Debug.Log($"Jogador morreu. Restam: {_remainingPlayers} jogadores");

        // Atualiza requisitos das portas IMEDIATAMENTE
        UpdateDoorRequirements();

        if (_remainingPlayers >= 0)
        {
            _isRunEnding = true;
            _victoryEnding = false;
            EndRun();
        }
    }

    public void EndRun()
    {
        CleanupPreviousRun();

        if (runEndCanvas != null) runEndCanvas.SetActive(true);

        if (_victoryEnding && victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            // Não chamamos StartAuction aqui, pois agora o botão de vitória vai chamar o RunSummary
        }
        else if (defeatPanel != null) defeatPanel.SetActive(true);

        SaveGame();
        if (timerCanvas != null) timerCanvas.SetActive(false);
        HideBar();
    }

    private void StartAuction()
    {
        // Este método não é mais necessário, mantido para compatibilidade
        // A lógica de leilão agora é tratada pelo RunSummary
    }

    private void ShowBar()
    {
        if (healthBarObject1 != null & !isMultiplayer)
        {
            healthBarObject1.SetActive(true);
        }
        else
        {
            healthBarObject1.SetActive(true);    
            healthBarObject2.SetActive(true);

        }
    }

    private void HideBar()
    {
        if (healthBarObject1 != null & !isMultiplayer)
        {
            healthBarObject1.SetActive(false);
        }
        else
        {
            healthBarObject1.SetActive(false);
            healthBarObject2.SetActive(false);
        }
    }

    private void OnVictoryToAuction()
    {
        runEndCanvas?.SetActive(false);
        victoryPanel?.SetActive(false);

        // Ativa o RunSummary para mostrar as ofertas baseadas nos itens coletados
        if (runSummary != null)
        {
            runSummary.ShowSummary();
        }
        else
        {
            Debug.LogError("RunSummary não atribuído no GameManager!");
        }
    }

    private void OnDefeatToMoonbox()
    {
        runEndCanvas?.SetActive(false);
        defeatPanel?.SetActive(false);
        customizationCanvas?.SetActive(true);

        // Reposiciona a câmera no moonbox
        if (slotMoonbox != null && Camera.main != null)
        {
            Camera.main.transform.position = slotMoonbox.position + Vector3.back * 10f;
        }
    }

    private void OnDefeatToMainMenu() => SceneManager.LoadScene("MainMenu");

    private void OnDefeatQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    #region Persistent UI & Stats
    public TextMeshProUGUI[] moneyTexts;
    public TextMeshProUGUI[] reputationTexts;
    public TextMeshProUGUI[] itemCountTexts;
    private int _collectedItems;

    public void UpdatePersistentStatsUI()
    {
        var m = PlayerRuntimeData.GetMoney().ToString();
        var r = PlayerRuntimeData.GetReputation().ToString();
        foreach (var t in moneyTexts) if (t) t.text = m;
        foreach (var t in reputationTexts) if (t) t.text = r;
    }

    private void ResetStatsUI()
    {
        _collectedItems = 0;
        foreach (var t in itemCountTexts) if (t) t.text = "0";
    }

    public void RegisterScriptableObject(int amount)
    {
        _collectedItems += amount;
        foreach (var t in itemCountTexts) if (t) t.text = _collectedItems.ToString();
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
                lastSaveTime = DateTime.UtcNow.ToString("o")
            };
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[GameManager] Falha ao salvar jogo: " + ex.Message);
        }
    }

    public void LoadGame()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                var data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(SavePath));
                PlayerRuntimeData.Initialize(data.savedMoney, data.savedReputation);
            }
            catch (Exception ex)
            {
                PlayerRuntimeData.Reset();
            }
        }
        else
        {
            PlayerRuntimeData.Reset();
        }
    }
    #endregion
}

[System.Serializable]
public class DoorState
{
    public bool requiresTwoPlayers = true;
    public bool isOpen = false;
    public event Action<DoorState> OnStateChanged;

    public void SetOpen(bool open)
    {
        if (isOpen == open) return;
        isOpen = open;
        OnStateChanged?.Invoke(this);
    }
}