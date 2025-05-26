using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    public Vector2Int GridCoord { get; private set; }
    public Transform cameraSlot;
    public Transform playerSpawnPoint;
    public float RoomWidth { get; private set; }
    public float RoomHeight { get; private set; }

    [Header("Portas")]
    [SerializeField] private GameObject doorUp;
    [SerializeField] private GameObject doorDown;
    [SerializeField] private GameObject doorLeft;
    [SerializeField] private GameObject doorRight;

    private List<DoorState> _doorStates = new List<DoorState>();
    private List<DoorTrigger> _doorTriggers = new List<DoorTrigger>();

    [Header("Inimigos")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int minEnemies = 1;
    [SerializeField] private int maxEnemies = 5;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    [Header("Baús")]
    [Tooltip("Slots possíveis para baús")]
    [SerializeField] private Transform[] chestSlots;
    [Tooltip("Chance de spawn de um baú em cada slot (0-1)")]
    [SerializeField] private float chestSpawnChance = 0.5f;
    [Tooltip("Chance de um baú ser um mími​co (0-1), caso spawnado")]
    [SerializeField] private float mimicChance = 0.2f;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject mimicPrefab;
    private List<GameObject> spawnedChests = new List<GameObject>();

    public void Initialize(Vector2Int coord, float width, float height)
    {
        GridCoord = coord;
        RoomWidth = width;
        RoomHeight = height;
        SetupDoors();
        SpawnEnemies();
        SpawnChests();
    }

    void SetupDoors()
    {
        Vector2Int gridSize = GameManager.Instance.GridSize;
        SetupDoor(doorUp, DoorDirection.Up, GridCoord.y < gridSize.y - 1);
        SetupDoor(doorDown, DoorDirection.Down, GridCoord.y > 0);
        SetupDoor(doorLeft, DoorDirection.Left, GridCoord.x > 0);
        SetupDoor(doorRight, DoorDirection.Right, GridCoord.x < gridSize.x - 1);
        InitializeDoorStates();
    }

    void SetupDoor(GameObject doorObj, DoorDirection direction, bool active)
    {
        if (doorObj == null) return;
        doorObj.SetActive(active);
        if (!active) return;

        var trigger = doorObj.GetComponent<DoorTrigger>();
        if (trigger != null)
        {
            trigger.direction = direction;
            _doorTriggers.Add(trigger);
        }

        GameManager.Instance.RegisterDoor(GridCoord, direction);
        _doorStates.Add(new DoorState { direction = direction });
    }

    void InitializeDoorStates()
    {
        foreach (var ds in _doorStates)
        {
            ds.isLocked = true;
            ds.isOpen = false;
        }
    }

    public DoorTrigger GetDoorTrigger(DoorDirection direction)
    {
        return _doorTriggers.Find(d => d.direction == direction);
    }

    void SpawnEnemies()
    {
        if (enemyPrefabs.Length == 0) return;
        int count = Random.Range(minEnemies, maxEnemies + 1);
        for (int i = 0; i < count; i++)
        {
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Vector2 rnd = new Vector2(
                Random.Range(-RoomWidth / 2f + 1, RoomWidth / 2f - 1),
                Random.Range(-RoomHeight / 2f + 1, RoomHeight / 2f - 1)
            );
            var e = Instantiate(prefab, transform.position + (Vector3)rnd, Quaternion.identity, transform);
            spawnedEnemies.Add(e);
        }
    }

    void SpawnChests()
    {
        if (chestSlots == null || chestSlots.Length == 0) return;
        foreach (var slot in chestSlots)
        {
            if (Random.value <= chestSpawnChance)
            {
                bool isMimic = Random.value <= mimicChance;
                GameObject toSpawn = isMimic ? mimicPrefab : chestPrefab;
                if (toSpawn != null)
                {
                    var c = Instantiate(toSpawn, slot.position, slot.rotation, transform);
                    spawnedChests.Add(c);
                }
            }
        }
    }

    public void SetEnemiesActive(bool active)
    {
        foreach (var e in spawnedEnemies)
            if (e != null)
                e.SetActive(active);
    }

    public void SetChestsActive(bool active)
    {
        foreach (var c in spawnedChests)
            if (c != null)
                c.SetActive(active);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetEnemiesActive(true);
            SetChestsActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetEnemiesActive(false);
            SetChestsActive(false);
        }
    }

    public DoorState GetDoorState(DoorDirection direction)
    {
        return _doorStates.Find(d => d.direction == direction);
    }

    public void SetActiveDoors(bool active)
    {
        foreach (var dt in _doorTriggers)
            dt.enabled = active;
    }

    public Vector3 GetPlayerSpawnPoint()
    {
        return playerSpawnPoint != null ? playerSpawnPoint.position : transform.position;
    }
}
