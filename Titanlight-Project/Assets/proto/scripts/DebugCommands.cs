using UnityEngine;

public class DebugCommands : MonoBehaviour
{
    private void Start()
    {
        // Usa apenas Vector2Int, sem converter para Vector3
        Vector2Int currentCoord = GameManager.Instance.GetCurrentRoomCoord();
        Debug.Log($"[Debug] Sala atual: {currentCoord}");
    }
}
