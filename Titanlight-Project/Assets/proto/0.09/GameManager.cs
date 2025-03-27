using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : Singleton<GameManager>
{
    #region Grid Settings
    [Header("Grid Parameters")]
    [SerializeField] private Vector2Int[] _gridPresets = {
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
    #endregion

    #region Room Settings
    [Header("Room Configuration")]
    public GameObject initialRoomPrefab;
    public List<RoomOption> roomOptions;
    
    private Dictionary<Vector2Int, Room> _rooms = new Dictionary<Vector2Int, Room>();
    private Vector2Int _currentRoomCoord;
    private Vector2Int _initialRoomCoord;
    #endregion

    #region Door System
    [Header("Door Management")]
    private Dictionary<Vector2Int, HashSet<DoorDirection>> _doors = new Dictionary<Vector2Int, HashSet<DoorDirection>>();
    #endregion

    #region Player & Camera
    private Camera _mainCamera;
    private bool _isTransitioning = false;
    public event Action<Vector2Int> OnRoomChanged;
    #endregion

    [Serializable]
    public class RoomOption
    {
        public string roomName;
        public GameObject roomPrefab;
        [Range(0, 100)] public float spawnChance;
    }

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
        PositionPlayer();
    }

    private void SetupGrid()
    {
        GridSize = _gridPresets[UnityEngine.Random.Range(0, _gridPresets.Length)];
        Debug.Log($"Selected Grid: {GridSize.x}x{GridSize.y}");

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

        GameObject prefab = coord == _initialRoomCoord ? 
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
            if (randomValue <= cumulative) return option.roomPrefab;
        }
        return roomOptions[0].roomPrefab;
    }

    private float CalculateTotalSpawnChance()
    {
        float total = 0f;
        foreach (RoomOption option in roomOptions) total += option.spawnChance;
        return total;
    }

    private void PositionPlayer()
    {
        if (Player.Instance == null) return;
        
        Player.Instance.transform.position = GetRoomWorldPosition(_initialRoomCoord);
        Debug.Log($"Player started in room: {_initialRoomCoord}");
    }

    #region Public Methods
    public void RegisterDoor(Vector2Int roomCoord, DoorDirection direction)
    {
        if (!_doors.ContainsKey(roomCoord))
            _doors[roomCoord] = new HashSet<DoorDirection>();
        
        _doors[roomCoord].Add(direction);
    }

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

    public Room GetRoom(Vector2Int coord) => _rooms.ContainsKey(coord) ? _rooms[coord] : null;
    public Vector2Int GetCurrentRoomCoord() => _currentRoomCoord;
    #endregion

    #region Room Transition
    public void SetCurrentRoom(Vector2Int coord)
    {
        _currentRoomCoord = coord;
        OnRoomChanged?.Invoke(coord);

        foreach (var room in _rooms.Values) room.SetActiveDoors(false);
        if (_rooms.TryGetValue(coord, out Room currentRoom)) currentRoom.SetActiveDoors(true);
        
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (_rooms.TryGetValue(_currentRoomCoord, out Room room) && room.cameraSlot != null)
            _mainCamera.transform.position = room.cameraSlot.position + new Vector3(0, 0, -10);
    }

    public void TryMoveThroughDoor(DoorDirection direction, float moveDistance)
    {
        Vector2Int dirVector = direction.ToVector();
        StartCoroutine(TransitionThroughDoor(dirVector, moveDistance));
    }

    private IEnumerator TransitionThroughDoor(Vector2Int direction, float moveDistance)
    {
        if (_isTransitioning) yield break;
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
    #endregion
}