[System.Serializable]
public class DoorState
{
    public DoorDirection direction;
    public bool isOpen = true;
    public bool isLocked = false; // Agora começa trancada por padrão
    public bool wasHacked = false; // Novo campo para rastrear se foi hackeada
}