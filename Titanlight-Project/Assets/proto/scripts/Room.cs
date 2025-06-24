using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    public Vector2Int RoomCoord { get; private set; }
    public Transform cameraSlot;
    public Transform playerSpawnPoint;
    public float RoomWidth { get; private set; }
    public float RoomHeight { get; private set; }

    [Header("Portas")]
    [SerializeField] private GameObject doorUp;
    [SerializeField] private GameObject doorDown;
    [SerializeField] private GameObject doorLeft;
    [SerializeField] private GameObject doorRight;

    [Header("Inimigos")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int minEnemies = 1;
    [SerializeField] private int maxEnemies = 5;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    [Header("Configuração de Baus")]
    [SerializeField] private Transform[] chestSpawnPoints;
    [SerializeField] private GameObject realChestPrefab;
    [SerializeField] private GameObject mimicChestPrefab;

    [Tooltip("Chance de um baú aparecer em cada ponto de spawn")]
    [SerializeField][Range(0, 1)] private float chestSpawnChance = 0.7f;

    [Tooltip("Chance de um baú ser um mimic")]
    [SerializeField][Range(0, 1)] private float mimicChance = 0.3f;

    private List<GameObject> spawnedChests = new List<GameObject>();

    [Header("Collider da Sala")]
    [SerializeField] public Collider2D roomCollider;

    public void Initialize(Vector2Int coord, float width, float height)
    {
        RoomCoord = coord;
        RoomWidth = width;
        RoomHeight = height;
        SetupDoors();
        SpawnEnemies();
        SpawnChests();
    }

    void SetupDoors()
    {
        Vector2Int gridSize = GameManager.Instance.GridSize;
        SetupDoor(doorUp, DoorDirection.Up, RoomCoord.y < gridSize.y - 1);
        SetupDoor(doorDown, DoorDirection.Down, RoomCoord.y > 0);
        SetupDoor(doorLeft, DoorDirection.Left, RoomCoord.x > 0);
        SetupDoor(doorRight, DoorDirection.Right, RoomCoord.x < gridSize.x - 1);
    }

    void SetupDoor(GameObject doorObj, DoorDirection direction, bool active)
    {
        if (doorObj == null) return;
        doorObj.SetActive(active);
        if (!active) return;

        DoorTrigger trigger = doorObj.GetComponent<DoorTrigger>();
        if (trigger != null)
        {
            trigger.direction = direction;

            // Configuração simplificada - pontos de spawn já devem estar no prefab
            Debug.Log($"Porta {direction} na sala {RoomCoord} ativada com spawn points: " +
                      $"P1: {trigger.player1SpawnPoint}, P2: {trigger.player2SpawnPoint}");
        }

        GameManager.Instance.RegisterDoor(RoomCoord, direction);
    }

    public DoorTrigger GetDoorTrigger(DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.Up:
                return doorUp?.GetComponent<DoorTrigger>();
            case DoorDirection.Down:
                return doorDown?.GetComponent<DoorTrigger>();
            case DoorDirection.Left:
                return doorLeft?.GetComponent<DoorTrigger>();
            case DoorDirection.Right:
                return doorRight?.GetComponent<DoorTrigger>();
            default:
                return null;
        }
    }

    void SpawnEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        spawnedEnemies.Clear();

        if (enemyPrefabs.Length == 0) return;

        int count = UnityEngine.Random.Range(minEnemies, maxEnemies + 1);
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
            Vector2 rnd = new Vector2(
                UnityEngine.Random.Range(-RoomWidth / 2f + 1, RoomWidth / 2f - 1),
                UnityEngine.Random.Range(-RoomHeight / 2f + 1, RoomHeight / 2f - 1)
            );
            GameObject enemy = Instantiate(prefab, transform.position + (Vector3)rnd, Quaternion.identity, transform);
            spawnedEnemies.Add(enemy);
            enemy.SetActive(false);
        }
    }

    void SpawnChests()
    {
        foreach (GameObject chest in spawnedChests)
        {
            if (chest != null) Destroy(chest);
        }
        spawnedChests.Clear();

        if (chestSpawnPoints == null || chestSpawnPoints.Length == 0) return;

        foreach (Transform spawnPoint in chestSpawnPoints)
        {
            if (UnityEngine.Random.value > chestSpawnChance) continue;

            if (UnityEngine.Random.value <= mimicChance && mimicChestPrefab != null)
            {
                GameObject mimic = Instantiate(
                    mimicChestPrefab,
                    spawnPoint.position,
                    Quaternion.identity,
                    transform
                );
                spawnedChests.Add(mimic);
                spawnedEnemies.Add(mimic);
                mimic.SetActive(false);
            }
            else if (realChestPrefab != null)
            {
                GameObject chest = Instantiate(
                    realChestPrefab,
                    spawnPoint.position,
                    Quaternion.identity,
                    transform
                );
                spawnedChests.Add(chest);
                chest.SetActive(false);
            }
        }
    }

    public void SetEnemiesActive(bool active)
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null) enemy.SetActive(active);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player 1") || other.CompareTag("player 2"))
        {
            SetEnemiesActive(true);
            foreach (GameObject chest in spawnedChests)
            {
                if (chest != null) chest.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("player 1") || other.CompareTag("player 2"))
        {
            SetEnemiesActive(false);
            foreach (GameObject chest in spawnedChests)
            {
                if (chest != null && chest.CompareTag("Chest"))
                    chest.SetActive(false);
            }
        }
    }

    public Vector3 GetPlayerSpawnPoint()
    {
        if (playerSpawnPoint == null)
        {
            Debug.LogWarning($"Room {RoomCoord} missing spawn point! Using center");
            return transform.position;
        }
        return playerSpawnPoint.position;
    }

    public Vector3 GetSpawnPointByDoorDirection(DoorDirection entryDirection)
    {
        DoorDirection oppositeDirection = GetOppositeDirection(entryDirection);
        return GetDoorSpawnPosition(oppositeDirection);
    }

    private DoorDirection GetOppositeDirection(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.Up: return DoorDirection.Down;
            case DoorDirection.Down: return DoorDirection.Up;
            case DoorDirection.Left: return DoorDirection.Right;
            case DoorDirection.Right: return DoorDirection.Left;
            default: return DoorDirection.Up;
        }
    }

    private Vector3 GetDoorSpawnPosition(DoorDirection dir)
    {
        DoorTrigger door = GetDoorTrigger(dir);
        if (door != null && door.player1SpawnPoint != null)
            return door.player1SpawnPoint.position;

        return GetPlayerSpawnPoint();
    }

    public Vector3 GetRandomPositionInRoom()
    {
        if (roomCollider == null)
        {
            Debug.LogWarning("Room collider is missing! Using center position.");
            return transform.position;
        }

        Bounds bounds = roomCollider.bounds;
        float x = UnityEngine.Random.Range(bounds.min.x + 1f, bounds.max.x - 1f);
        float y = UnityEngine.Random.Range(bounds.min.y + 1f, bounds.max.y - 1f);

        return new Vector3(x, y, 0f);
    }

    public Collider2D GetRoomCollider()
    {
        return roomCollider;
    }
}