namespace DungeonSystem
{
    [System.Serializable]
    public class DoorState
    {
        public DoorDirection direction;
        public bool isOpen = false;
        public bool isLocked = true;
        public bool exists = true;
    }
}