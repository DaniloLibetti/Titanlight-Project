using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using Player.StateMachine;

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

    private Dictionary<Vector2Int, HashSet<DoorDirection>> _doors =
        new Dictionary<Vector2Int, HashSet<DoorDirection>>();

    #endregion

    #region UI / HUD

    [Header("UI Inicial")]
    public GameObject customizationCanvas;

    [Header("HUD do Jogador")]
    public GameObject playerStatusCanvas;
    public GameObject playerOtherCanvas;
    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI reputationText;

    // Contagem interna de itens coletados
    private int _collectedItems = 0;
    // exposto para ShopMenu, RunSummary
    public int ScriptableObjectCount => _collectedItems;

    [Header("Leilão")]
    public Transform slotAuction;
    public GameObject auctionCanvas;

    #endregion

    #region Câmera

    private Camera _mainCamera;

    #endregion

    #region Controles de Tempo

    private float _shiftHoldTimer = 0f;
    private const float SHIFT_HOLD_DURATION = 2f;

    #endregion

    #region Singleton & Inicialização

    protected override void Awake()
    {
        base.Awake();
        _mainCamera = Camera.main;

        LoadGame();

        // UI inicial
        customizationCanvas.SetActive(true);
        playerStatusCanvas.SetActive(false);
        playerOtherCanvas.SetActive(false);
        if (auctionCanvas) auctionCanvas.SetActive(false);

        UpdatePersistentStatsUI();
        if (itemCountText) itemCountText.text = "0";
    }

    #endregion

    #region StartRun

    public void StartRun()
    {
        // reset itens
        _collectedItems = 0;
        if (itemCountText) itemCountText.text = "0";

        // UI
        customizationCanvas.SetActive(false);
        playerStatusCanvas.SetActive(true);
        playerOtherCanvas.SetActive(true);

        // gerar mapa
        SetupGrid();
        GenerateWorld();
        PairDoors();
        SetCurrentRoom(_initialRoomCoord);

        // spawn players
        Room startRoom = GetRoom(_initialRoomCoord);
        if (startRoom != null)
        {
            Vector3 spawnPos = startRoom.GetPlayerSpawnPoint();
            spawnPos.z = 0f;
            PlayerManager.Instance.SpawnPlayer(spawnPos);
        }
    }

    #endregion

    #region Update (Shift para encerrar)

    private void Update()
    {
        if (Input.GetKey(endRunKey))
        {
            _shiftHoldTimer += Time.deltaTime;
            if (_shiftHoldTimer >= SHIFT_HOLD_DURATION)
            {
                EndRunAndAuction();
                _shiftHoldTimer = 0f;
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
        GridSize = _gridPresets[UnityEngine.Random.Range(0, _gridPresets.Length)];
        _gridOffset = new Vector3(
            -(GridSize.x * roomWidth) / 2f + roomWidth / 2f,
            -(GridSize.y * roomHeight) / 2f + roomHeight / 2f,
            0f
        );
    }

    private void GenerateWorld()
    {
        _initialRoomCoord = new Vector2Int(
            UnityEngine.Random.Range(0, GridSize.x),
            UnityEngine.Random.Range(0, GridSize.y)
        );

        float totalChance = 0f;
        foreach (var o in roomOptions) totalChance += o.spawnChance;

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                CreateRoom(new Vector2Int(x, y), totalChance);
    }

    private void CreateRoom(Vector2Int coord, float totalChance)
    {
        Vector3 pos = new Vector3(coord.x * roomWidth, coord.y * roomHeight, 0f) + _gridOffset;
        GameObject prefab = (coord == _initialRoomCoord)
            ? initialRoomPrefab
            : SelectRandomRoomPrefab(totalChance);
        Room room = Instantiate(prefab, pos, Quaternion.identity).GetComponent<Room>();
        room.Initialize(coord, roomWidth, roomHeight);
        _rooms.Add(coord, room);
    }

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
        var n = GetRoom(coord + off)?.GetDoorTrigger(opp);
        if (d != null && n != null)
        {
            d.pairedDoor = n;
            n.pairedDoor = d;
        }
    }

    public bool IsDoorAccessible(Vector2Int coord, DoorDirection dir)
        => _doors.ContainsKey(coord) && _doors[coord].Contains(dir);

    #endregion

    #region Troca de Sala (Wrapper)

    public void TryMoveThroughDoor(DoorDirection dir, float dist)
    {
        PlayerManager.Instance.TryMoveThroughDoor(dir, dist);
    }

    #endregion

    #region Sala Atual & Câmera

    public Vector2Int GetCurrentRoomCoord() => _currentRoomCoord;

    public Room GetRoom(Vector2Int coord)
        => _rooms.TryGetValue(coord, out var r) ? r : null;

    public void SetCurrentRoom(Vector2Int coord)
    {
        _currentRoomCoord = coord;
        UpdateCameraPosition();
        if (_rooms.TryGetValue(coord, out var room))
            room.SetActiveDoors(true);
        OnRoomChanged?.Invoke(coord);
    }

    private void UpdateCameraPosition()
    {
        if (_rooms.TryGetValue(_currentRoomCoord, out var room) && room.cameraSlot != null)
            _mainCamera.transform.position = room.cameraSlot.position + Vector3.back * 10f;
    }

    public event Action<Vector2Int> OnRoomChanged;

    #endregion

    #region EndRun & Auction

    public void EndRunAndAuction()
    {
        // destrói salas e limpa dicionários
        foreach (var r in _rooms.Values)
            if (r) Destroy(r.gameObject);
        _rooms.Clear();
        _doors.Clear();

        // exibe leilão
        if (auctionCanvas) auctionCanvas.SetActive(true);
        if (slotAuction != null)
            _mainCamera.transform.position = slotAuction.position + Vector3.back * 10f;

        // mostra resumo e salva                   preciso rever essa parte pois nao ta funcionando corretamente desde o multiplayer
        FindObjectOfType<RunSummary>()?.ShowSummary();
        SaveGame();
    }

    public void CompleteAuction()
    {
        if (auctionCanvas) auctionCanvas.SetActive(false);
        if (customizationCanvas) customizationCanvas.SetActive(true);

        UpdatePersistentStatsUI();
        SaveGame();
    }

    #endregion

    #region UI Atualização

    public void UpdatePersistentStatsUI()
    {
        if (moneyText) moneyText.text = PlayerRuntimeData.GetMoney().ToString();
        if (reputationText) reputationText.text = PlayerRuntimeData.GetReputation().ToString();
    }

    public void RegisterScriptableObject(int amount)
    {
        _collectedItems += amount;
        if (itemCountText) itemCountText.text = _collectedItems.ToString();
    }

    #endregion

    #region Save/Load

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public void SaveGame()
    {
        var data = new PlayerSaveData
        {
            savedMoney = PlayerRuntimeData.GetMoney(),
            savedReputation = PlayerRuntimeData.GetReputation()
        };
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log($"Game Saved | Money: {data.savedMoney} | Rep: {data.savedReputation}");
    }

    public void LoadGame()
    {
        if (File.Exists(SavePath))
        {
            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<PlayerSaveData>(json);
            PlayerRuntimeData.Initialize(data.savedMoney, data.savedReputation);
            Debug.Log($"Game Loaded | Money: {data.savedMoney} | Rep: {data.savedReputation}");
        }
        else
        {
            Debug.Log("New Game Started");
            PlayerRuntimeData.Reset();
        }
    }

    #endregion

    #region Main Menu

    public void GoToMainMenu() => SceneManager.LoadScene("MainMenu");

    #endregion
}
