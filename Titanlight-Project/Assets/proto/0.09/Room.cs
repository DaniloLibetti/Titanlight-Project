using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    // Informações básicas da sala
    public Vector2Int GridCoord { get; private set; }
    public Transform cameraSlot;
    public float RoomWidth { get; private set; }
    public float RoomHeight { get; private set; }

    [Header("Portas (atribua pelo Inspector)")]
    [SerializeField] private GameObject doorUp;
    [SerializeField] private GameObject doorDown;
    [SerializeField] private GameObject doorLeft;
    [SerializeField] private GameObject doorRight;

    // Listas para controlar o estado e os triggers das portas
    private List<DoorState> _doorStates = new List<DoorState>();
    private List<DoorTrigger> _doorTriggers = new List<DoorTrigger>();

    [Header("Spawn de Inimigos")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int minEnemies = 1;
    [SerializeField] private int maxEnemies = 5;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private bool playerInside = false;

    // Inicializa a sala com as configurações passadas
    public void Initialize(Vector2Int coord, float width, float height)
    {
        GridCoord = coord;
        RoomWidth = width;
        RoomHeight = height;
        SetupDoors();
        SpawnEnemies();
    }

    // Configura as portas com base na existência de sala vizinha
    void SetupDoors()
    {
        Vector2Int gridSize = GameManager.Instance.GridSize;

        // Verifica cada direção para ativar ou desativar a porta
        SetupDoor(doorUp, DoorDirection.Up, GridCoord.y < gridSize.y - 1);
        SetupDoor(doorDown, DoorDirection.Down, GridCoord.y > 0);
        SetupDoor(doorLeft, DoorDirection.Left, GridCoord.x > 0);
        SetupDoor(doorRight, DoorDirection.Right, GridCoord.x < gridSize.x - 1);

        InitializeDoorStates();
    }

    // Configura uma porta: ativa/desativa e registra seu trigger e estado se ativa
    void SetupDoor(GameObject doorObj, DoorDirection direction, bool shouldBeActive)
    {
        if (doorObj == null)
        {
            Debug.LogWarning("Uma porta não foi atribuída no Inspector em " + gameObject.name);
            return;
        }

        doorObj.SetActive(shouldBeActive);

        if (shouldBeActive)
        {
            // Registra o DoorTrigger, se existir
            DoorTrigger trigger = doorObj.GetComponent<DoorTrigger>();
            if (trigger != null)
            {
                trigger.direction = direction;
                _doorTriggers.Add(trigger);
            }
            // Registra a porta no GameManager e adiciona seu estado
            GameManager.Instance.RegisterDoor(GridCoord, direction);
            _doorStates.Add(new DoorState { direction = direction });
        }
    }

    // Inicializa os estados das portas (todas começam travadas e fechadas)
    void InitializeDoorStates()
    {
        foreach (DoorState door in _doorStates)
        {
            door.isLocked = true;
            door.isOpen = false;
        }
    }

    // Retorna o DoorTrigger da porta que tem a direção informada
    public DoorTrigger GetDoorTrigger(DoorDirection direction)
    {
        return _doorTriggers.Find(d => d.direction == direction);
    }

    // Spawna inimigos aleatórios na sala
    void SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return;

        int count = Random.Range(minEnemies, maxEnemies + 1);

        for (int i = 0; i < count; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Vector2 randomPos = new Vector2(
                Random.Range(-RoomWidth / 2f + 1f, RoomWidth / 2f - 1f),
                Random.Range(-RoomHeight / 2f + 1f, RoomHeight / 2f - 1f)
            );
            GameObject enemy = Instantiate(enemyPrefab, transform.position + (Vector3)randomPos, Quaternion.identity, transform);
            spawnedEnemies.Add(enemy);
        }
    }

    // Ativa/desativa inimigos conforme o jogador entra ou sai
    public void SetEnemiesActive(bool active)
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
                enemy.SetActive(active);
        }
    }

    // Detecta entrada do jogador na sala
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            SetEnemiesActive(true);
        }
    }

    // Detecta saída do jogador da sala
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            SetEnemiesActive(false);
        }
    }

    // Permite que outros scripts acessem o estado da porta com base na direção
    public DoorState GetDoorState(DoorDirection direction)
    {
        return _doorStates.Find(d => d.direction == direction);
    }

    // Ativa ou desativa os triggers das portas (por exemplo, ao entrar na sala)
    public void SetActiveDoors(bool active)
    {
        foreach (DoorTrigger trigger in _doorTriggers)
        {
            trigger.enabled = active;
        }
    }
}
