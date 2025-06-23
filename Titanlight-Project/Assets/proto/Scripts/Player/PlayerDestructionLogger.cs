using UnityEngine;

public class PlayerDestructionLogger : MonoBehaviour
{
    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            string stackTrace = System.Environment.StackTrace;
            Debug.LogError($"[DESTRUCTION] {name} destroyed! ID: {GetInstanceID()}\n{stackTrace}");
        }
    }
}