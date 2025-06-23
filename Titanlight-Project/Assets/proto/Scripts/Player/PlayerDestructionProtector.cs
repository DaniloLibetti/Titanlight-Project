using UnityEngine;

public class PlayerDestructionProtector : MonoBehaviour
{
    private PlayerManager _manager;
    private bool _canDestroy = false;

    public void Initialize(PlayerManager manager)
    {
        _manager = manager;
    }

    public void AllowDestruction()
    {
        _canDestroy = true;
    }

    private void OnDestroy()
    {
        if (!_canDestroy && _manager != null)
        {
            _manager.ReSpawnPlayers();
        }
    }
}