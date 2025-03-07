using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    public Vector2Int GridCoord { get; private set; }
    public Transform cameraSlot;
    public float RoomWidth { get; private set; }
    public float RoomHeight { get; private set; }

    private List<DoorState> _doorStates = new List<DoorState>();
    private List<DoorTrigger> _doorTriggers = new List<DoorTrigger>(); // Armazena os triggers das portas

    public void Initialize(Vector2Int coord, float width, float height)
    {
        GridCoord = coord;
        RoomWidth = width;
        RoomHeight = height;
        SetupDoors();
        Debug.Log($"[Room] Sala {coord} inicializada com largura {width} e altura {height}.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Movendo a câmera para a posição do cameraSlot
            Camera.main.transform.position = cameraSlot.position;
            Camera.main.transform.rotation = cameraSlot.rotation;
        }
    }

    void SetupDoors()
    {
        Vector2Int potentialCoord;

        // Porta para cima
        potentialCoord = GridCoord + Vector2Int.up;
        if (potentialCoord.y < GameManager.Instance.gridSize.y)
            CreateDoor(DoorDirection.Up, new Vector2(0, RoomHeight / 2));

        // Porta para baixo
        potentialCoord = GridCoord + Vector2Int.down;
        if (potentialCoord.y >= 0)
            CreateDoor(DoorDirection.Down, new Vector2(0, -RoomHeight / 2));

        // Porta para a esquerda
        potentialCoord = GridCoord + Vector2Int.left;
        if (potentialCoord.x >= 0)
            CreateDoor(DoorDirection.Left, new Vector2(-RoomWidth / 2, 0));

        // Porta para a direita
        potentialCoord = GridCoord + Vector2Int.right;
        if (potentialCoord.x < GameManager.Instance.gridSize.x)
            CreateDoor(DoorDirection.Right, new Vector2(RoomWidth / 2, 0));
    }

    void CreateDoor(DoorDirection dir, Vector2 localPosition)
    {
        GameObject door = new GameObject($"Door_{dir}");
        door.transform.SetParent(transform);
        door.transform.localPosition = localPosition;

        BoxCollider2D collider = door.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);

        DoorTrigger trigger = door.AddComponent<DoorTrigger>();
        trigger.roomCoord = GridCoord;
        trigger.direction = dir;
        _doorTriggers.Add(trigger); // Guarda a referência do trigger

        GameManager.Instance.RegisterDoor(GridCoord, dir);
        _doorStates.Add(new DoorState { direction = dir });
    }

    public DoorState GetDoorState(DoorDirection dir)
    {
        return _doorStates.Find(d => d.direction == dir);
    }

    // Ativa ou desativa todos os triggers desta sala
    public void SetActiveDoors(bool active)
    {
        foreach (DoorTrigger trigger in _doorTriggers)
        {
            trigger.enabled = active;
        }
    }
}