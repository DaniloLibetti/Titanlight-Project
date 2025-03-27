[System.Serializable]
public class DoorState
{
    public DoorDirection direction;
    public bool isOpen = false; // Começa fechada
    public bool isLocked = true; // Começa trancada
}
