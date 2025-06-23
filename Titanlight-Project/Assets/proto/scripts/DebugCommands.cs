using UnityEngine;

public class DebugCommands : MonoBehaviour
{
    private void Start()
    {
        Vector2Int currentCoord = GameManager.Instance.GetCurrentRoomCoord();
        Debug.Log($"[Debug] Sala atual: {currentCoord}");
    }
}
