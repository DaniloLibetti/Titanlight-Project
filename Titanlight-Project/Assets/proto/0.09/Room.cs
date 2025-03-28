using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    // Informações básicas da sala
    public Vector2Int GridCoord { get; private set; }
    public Transform cameraSlot;
    public float RoomWidth { get; private set; }
    public float RoomHeight { get; private set; }

    // Listas para controlar portas
    private List<DoorState> _doorStates = new List<DoorState>();
    private List<DoorTrigger> _doorTriggers = new List<DoorTrigger>();

    [Header("Spawn de Inimigos")]
    [SerializeField] private GameObject[] enemyPrefabs; // Tipos de inimigos que podem nascer aqui
    [SerializeField] private int minEnemies = 1; // Mínimo de inimigos por sala
    [SerializeField] private int maxEnemies = 5; // Máximo de inimigos por sala
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // Lista dos inimigos criados

    // Controle se o jogador está dentro da sala
    private bool playerInside = false;

    // Configura valores iniciais quando a sala é criada
    public void Initialize(Vector2Int coord, float width, float height)
    {
        GridCoord = coord;
        RoomWidth = width;
        RoomHeight = height;
        SetupDoors(); // Cria as portas
        SpawnEnemies(); // Cria os inimigos
    }

    // Cria portas nas direções necessárias
    void SetupDoors()
    {
        Vector2Int gridSize = GameManager.Instance.GridSize;

        // Verifica em quais direções precisam de portas
        if (GridCoord.y < gridSize.y - 1)
            CreateDoor(DoorDirection.Up, new Vector2(0, RoomHeight / 2));

        if (GridCoord.y > 0)
            CreateDoor(DoorDirection.Down, new Vector2(0, -RoomHeight / 2));

        if (GridCoord.x > 0)
            CreateDoor(DoorDirection.Left, new Vector2(-RoomWidth / 2, 0));

        if (GridCoord.x < gridSize.x - 1)
            CreateDoor(DoorDirection.Right, new Vector2(RoomWidth / 2, 0));

        InitializeDoorStates();
    }

    // Define estado inicial das portas (todas fechadas)
    void InitializeDoorStates()
    {
        foreach (DoorState door in _doorStates)
        {
            door.isLocked = true;
            door.isOpen = false;
        }
    }

    // Cria uma porta física na cena
    void CreateDoor(DoorDirection dir, Vector2 localPosition)
    {
        GameObject door = new GameObject($"Door_{dir}");
        door.transform.SetParent(transform);
        door.transform.localPosition = localPosition;

        // Adiciona colisor para detectar jogador
        BoxCollider2D collider = door.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);

        // Configura o trigger da porta
        DoorTrigger trigger = door.AddComponent<DoorTrigger>();
        trigger.direction = dir;
        _doorTriggers.Add(trigger);

        // Registra a porta no GameManager
        GameManager.Instance.RegisterDoor(GridCoord, dir);
        _doorStates.Add(new DoorState { direction = dir });
    }

    // Cria inimigos aleatórios na sala
    void SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return;

        // Escolhe quantos inimigos criar
        int count = Random.Range(minEnemies, maxEnemies + 1);

        for (int i = 0; i < count; i++)
        {
            // Escolhe tipo de inimigo aleatório
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // Calcula posição aleatória dentro da sala
            Vector2 randomPos = new Vector2(
                Random.Range(-RoomWidth / 2f + 1f, RoomWidth / 2f - 1f),
                Random.Range(-RoomHeight / 2f + 1f, RoomHeight / 2f - 1f)
            );

            // Cria o inimigo e guarda na lista
            GameObject enemy = Instantiate(enemyPrefab, transform.position + (Vector3)randomPos, Quaternion.identity, transform);
            spawnedEnemies.Add(enemy);
        }
    }

    // Liga/desliga inimigos conforme o jogador entra/sai
    public void SetEnemiesActive(bool active)
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
                enemy.SetActive(active);
        }
    }

    // Detecta quando jogador entra na sala
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            SetEnemiesActive(true); // Ativa inimigos
        }
    }

    // Detecta quando jogador sai da sala
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            SetEnemiesActive(false); // Desativa inimigos
        }
    }

    // Métodos para acesso externo
    public DoorState GetDoorState(DoorDirection dir) => _doorStates.Find(d => d.direction == dir);

    public void SetActiveDoors(bool active)
    {
        foreach (DoorTrigger trigger in _doorTriggers)
        {
            trigger.enabled = active;
        }
    }
}