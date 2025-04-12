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
    private Vector2Int _initialRoomCoord;
    private Vector2Int _currentRoomCoord;

    [Header("Door Management")]
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

    [Header("UI & Slots")]
    public GameObject customizationCanvas; // Tela inicial (MoonBox)
    public Transform slotMoonBox;          // Posição do menu inicial
    public Transform slotStartCamera;      // Referência (não utilizado para spawn)

    // HUD do player
    public GameObject playerStatusCanvas;
    public GameObject playerOtherCanvas;

    [Header("Auction Settings")]
    [Tooltip("Transform para o Slot Leilão")]
    public Transform slotAuction;          // Posição da câmera durante o leilão
    [Tooltip("Canvas do leilão que apresenta as três ofertas")]
    public GameObject auctionCanvas;

    // Campo para contagem de moedas coletadas
    private int _coinCount = 0;
    public int CoinCount { get { return _coinCount; } }

    private float shiftHoldTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        _mainCamera = Camera.main;

        // Estado inicial: mostra o canvas de customização; demais HUDs e o canvas de leilão desativados
        customizationCanvas.SetActive(true);
        playerStatusCanvas.SetActive(false);
        playerOtherCanvas.SetActive(false);
        if (auctionCanvas != null)
            auctionCanvas.SetActive(false);

        // A câmera inicia posicionada no slot MoonBox
        _mainCamera.transform.position = slotMoonBox.position + Vector3.back * 10f;
    }

    /// <summary>
    /// Inicia uma nova run.
    /// </summary>
    public void StartRun()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // Reinicia o contador de moedas
        _coinCount = 0;

        // Desativa o canvas de customização e ativa os HUDs
        customizationCanvas.SetActive(false);
        playerStatusCanvas.SetActive(true);
        playerOtherCanvas.SetActive(true);

        SetupGrid();
        GenerateWorld();
        PairDoors();

        SetCurrentRoom(_initialRoomCoord);
        InstantiatePlayer();

        var timer = FindObjectOfType<CountdownTimer>();
        if (timer != null)
            timer.ResetTimer();
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
            for (int y = 0; y < GridSize.y; y++)
                CreateRoom(new Vector2Int(x, y), totalChance);
    }

    private void CreateRoom(Vector2Int coord, float totalChance)
    {
        Vector3 pos = new Vector3(coord.x * roomWidth, coord.y * roomHeight, 0) + _gridOffset;
        GameObject prefab = (coord == _initialRoomCoord) ? initialRoomPrefab : SelectRandomRoomPrefab(totalChance);
        Room room = Instantiate(prefab, pos, Quaternion.identity).GetComponent<Room>();
        room.Initialize(coord, roomWidth, roomHeight);
        _rooms.Add(coord, room);
    }

    private GameObject SelectRandomRoomPrefab(float totalChance)
    {
        float rnd = UnityEngine.Random.Range(0f, totalChance), cum = 0f;
        foreach (var opt in roomOptions)
        {
            cum += opt.spawnChance;
            if (rnd <= cum)
                return opt.roomPrefab;
        }
        return roomOptions[0].roomPrefab;
    }

    private float CalculateTotalSpawnChance()
    {
        float t = 0f;
        foreach (var o in roomOptions)
            t += o.spawnChance;
        return t;
    }

    private void InstantiatePlayer()
    {
        Room startRoom = GetRoom(_initialRoomCoord);
        if (startRoom == null)
        {
            Debug.LogError("Sala inicial não encontrada para spawn!");
            return;
        }

        Vector3 spawnPos = startRoom.GetPlayerSpawnPoint();
        spawnPos.z = 0;
        GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        var health = playerObj.GetComponent<Health>();
        if (health != null)
        {
            health.onDeath.AddListener(HandlePlayerDeath);
            var timer = FindObjectOfType<CountdownTimer>();
            if (timer != null)
                health.onDeath.AddListener(() => timer.OnPlayerDeath());
        }
    }

    /// <summary>
    /// Quando o player morrer, inicia o fluxo do leilão.
    /// </summary>
    private void HandlePlayerDeath()
    {
        StartAuction();
    }

    /// <summary>
    /// Inicia o leilão: a câmera se move para o slot de leilão e o canvas de leilão é ativado.
    /// </summary>
    private void StartAuction()
    {
        if (auctionCanvas != null)
            auctionCanvas.SetActive(true);
        if (slotAuction != null)
            _mainCamera.transform.position = slotAuction.position + Vector3.back * 10f;
    }

    /// <summary>
    /// Quando o player escolhe uma oferta no leilão, este método é chamado para resetar o jogo.
    /// </summary>
    public void CompleteAuction()
    {
        if (auctionCanvas != null)
            auctionCanvas.SetActive(false);
        ResetToInitialState();
    }

    /// <summary>
    /// Reseta o estado do jogo:
    /// – limpa as salas da run,
    /// – reposiciona a câmera no slot MoonBox,
    /// – ativa o canvas de customização.
    /// </summary>
    private void ResetToInitialState()
    {
        ExitRun();
        if (slotMoonBox != null)
            _mainCamera.transform.position = slotMoonBox.position + Vector3.back * 10f;
        if (customizationCanvas != null)
            customizationCanvas.SetActive(true);
    }

    public void RegisterDoor(Vector2Int coord, DoorDirection dir)
    {
        if (!_doors.ContainsKey(coord))
            _doors[coord] = new HashSet<DoorDirection>();
        _doors[coord].Add(dir);
    }

    public bool IsDoorAccessible(Vector2Int coord, DoorDirection dir)
    {
        return _doors.ContainsKey(coord) && _doors[coord].Contains(dir);
    }

    public Vector2Int GetCurrentRoomCoord()
    {
        return _currentRoomCoord;
    }

    public Room GetRoom(Vector2Int coord)
    {
        return _rooms.TryGetValue(coord, out var r) ? r : null;
    }

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

    public void TryMoveThroughDoor(DoorDirection dir, float dist)
    {
        StartCoroutine(TransitionThroughDoor(dir.ToVector(), dist));
    }

    private IEnumerator TransitionThroughDoor(Vector2Int dir, float dist)
    {
        if (_isTransitioning)
            yield break;
        _isTransitioning = true;
        if (IsDoorAccessible(_currentRoomCoord, dir.ToDoorDirection()))
        {
            var col = Player.Instance.GetComponent<Collider2D>();
            col.enabled = false;
            Vector3 start = Player.Instance.transform.position;
            Vector3 offset = (Vector3)(Vector2)dir * dist;
            Vector3 target = start + offset;
            yield return SmoothTransition(col, start, target);
            SetCurrentRoom(_currentRoomCoord + dir);
        }
        _isTransitioning = false;
    }

    private IEnumerator SmoothTransition(Collider2D col, Vector3 a, Vector3 b)
    {
        float duration = 0.5f, t = 0f;
        while (t < duration)
        {
            Player.Instance.transform.position = Vector3.Lerp(a, b, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        Player.Instance.transform.position = b;
        col.enabled = true;
    }

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

    public void GoToMainMenu()
    {
        // Aqui carrega a cena do Main Menu, certifique-se que o nome da cena está correto e adicionada na Build Settings
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Limpa as salas e HUDs da run.
    /// </summary>
    private void ExitRun()
    {
        foreach (var r in _rooms.Values)
        {
            if (r != null)
                Destroy(r.gameObject);
        }
        _rooms.Clear();
        _doors.Clear();

        var timer = FindObjectOfType<CountdownTimer>();
        if (timer != null)
            timer.gameObject.SetActive(false);

        _mainCamera.transform.position = slotMoonBox.position + Vector3.back * 10f;
        playerStatusCanvas.SetActive(false);
        playerOtherCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            shiftHoldTimer += Time.deltaTime;
            if (shiftHoldTimer >= 3f)
            {
                ExitRun();
                shiftHoldTimer = 0f;
            }
        }
        else
        {
            shiftHoldTimer = 0f;
        }
    }

    /// <summary>
    /// Registra as moedas coletadas.
    /// </summary>
    public void RegisterCoin(int amount)
    {
        _coinCount += amount;
        Debug.Log("Moeda coletada. Total: " + _coinCount);
    }
}
