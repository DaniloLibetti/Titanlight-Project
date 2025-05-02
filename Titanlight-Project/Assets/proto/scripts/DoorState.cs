[System.Serializable]
public class DoorState
{
    public DoorDirection direction;
    public bool isOpen = false;
    public bool isLocked = true;
    // Nova flag para indicar se a porta existe (tem sprite) ou n�o
    public bool exists = true;
}
