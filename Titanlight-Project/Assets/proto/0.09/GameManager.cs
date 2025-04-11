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
    public GameObject customizationCanvas;
    public Transform slotMoonBox;
    public Transform slotStartCamera; // não mais usado para spawn, apenas referência

    // Novos Canvas para o status do jogador (certifique-se de que estão inativos inicialmente)
    public GameObject playerStatusCanvas;
    public GameObject playerOtherCanvas;

    [Header("Game Over Settings")]
    public GameObject gameOverPanel;

    private float shiftHoldTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        _mainCamera = Camera.main;

        // Inicialmente, exibe o canvas de customização e garante que os demais estejam inativos
        customizationCanvas.SetActive(true);
        gameOverPanel.SetActive(false);
        playerStatusCanvas.SetActive(false);
        playerOtherCanvas.SetActive(false);

        // A câmera inicia posicionada no slotMoonBox
        _mainCamera.transform.position = slotMoonBox.position + Vector3.back * 10f;
    }

    /// <summary>
    /// Método chamado ao iniciar a run (por exemplo, ao pressionar o botão "Iniciar Run").
    /// </summary>
    public void StartRun()
    {
        // Se o GameManager estiver inativo, ativa-o para que as corrotinas possam ser iniciadas.
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // Desativa o canvas de customização e ativa os canvases do status do jogador.
        customizationCanvas.SetActive(false);
        playerStatusCanvas.SetActive(true);
        playerOtherCanvas.SetActive(true);

        SetupGrid();
        GenerateWorld();
        PairDoors();

        // Ativa a sala inicial e posiciona a câmera
        SetCurrentRoom(_initialRoomCoord);

        // Instancia o player na sala inicial
        InstantiatePlayer();

        // Inicia ou reseta o timer, se houver
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
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                CreateRoom(new Vector2Int(x, y), totalChance);
            }
        }
    }

    private void CreateRoom(Vector2Int coord, float totalChance)
    {
        Vector3 pos = new Vector3(coord.x * roomWidth, coord.y * roomHeight, 0) + _gridOffset;
        GameObject prefab = coord == _initialRoomCoord
            ? initialRoomPrefab
            : SelectRandomRoomPrefab(totalChance);
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

    private void HandlePlayerDeath()
    {
        gameOverPanel.SetActive(true);
        ExitRun();
    }

    public void ReturnToMoonBox()
    {
        SceneManager.LoadScene("MoonBox");
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

        // Reseta a câmera para o slotMoonBox e restaura os canvases
        _mainCamera.transform.position = slotMoonBox.position + Vector3.back * 10f;
        customizationCanvas.SetActive(true);
        gameOverPanel.SetActive(false);

        // Desativa os canvases do status do jogador
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
}
