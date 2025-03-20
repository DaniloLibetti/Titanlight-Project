using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    public Vector2Int GridCoord { get; private set; }
    public Transform cameraSlot;
    public float RoomWidth { get; private set; }
    public float RoomHeight { get; private set; }

    private List<DoorState> _doorStates = new List<DoorState>();
    private List<DoorTrigger> _doorTriggers = new List<DoorTrigger>();

    public void Initialize(Vector2Int coord, float width, float height)
    {
        GridCoord = coord;
        RoomWidth = width;
        RoomHeight = height;
        SetupDoors();
    }

    void SetupDoors()
    {
        Vector2Int gridSize = GameManager.Instance.gridSize;

        // Cria as portas com base na posição da sala na grade
        if (GridCoord.y < gridSize.y - 1)
            CreateDoor(DoorDirection.Up, new Vector2(0, RoomHeight / 2));

        if (GridCoord.y > 0)
            CreateDoor(DoorDirection.Down, new Vector2(0, -RoomHeight / 2));

        if (GridCoord.x > 0)
            CreateDoor(DoorDirection.Left, new Vector2(-RoomWidth / 2, 0));

        if (GridCoord.x < gridSize.x - 1)
            CreateDoor(DoorDirection.Right, new Vector2(RoomWidth / 2, 0));

        // Inicializa o estado de cada porta (30% chance de estar trancada)
        InitializeDoorStates();
    }

    void InitializeDoorStates()
    {
        foreach (DoorState door in _doorStates)
        {
            door.isLocked = Random.value < 0.3f;
            door.isOpen = !door.isLocked;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Movimenta a câmera para a posição e rotação definidas pelo cameraSlot da sala
            Camera.main.transform.position = cameraSlot.position;
            Camera.main.transform.rotation = cameraSlot.rotation;
        }
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
        _doorTriggers.Add(trigger);

        GameManager.Instance.RegisterDoor(GridCoord, dir);
        _doorStates.Add(new DoorState { direction = dir });
    }

    public DoorState GetDoorState(DoorDirection dir)
    {
        return _doorStates.Find(d => d.direction == dir);
    }

    // Ativa ou desativa os triggers das portas desta sala
    public void SetActiveDoors(bool active)
    {
        foreach (DoorTrigger trigger in _doorTriggers)
        {
            trigger.enabled = active;
        }
    }

    // Restante dos métodos mantidos conforme necessário
}
