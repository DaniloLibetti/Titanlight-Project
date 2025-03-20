[System.Serializable]
public class DoorState
{
    public DoorDirection direction;
    public bool isOpen = true;
    public bool isLocked = false; // Agora come�a trancada por padr�o
    public bool wasHacked = false; // Novo campo para rastrear se foi hackeada
}