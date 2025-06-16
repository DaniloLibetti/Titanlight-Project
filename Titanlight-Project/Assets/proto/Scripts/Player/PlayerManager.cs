using UnityEngine;
using System.Collections;
using Player.StateMachine;  // ajuste se seu namespace for diferente

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("Prefabs dos Jogadores")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    [Header("Configuração de Modo")]
    public bool isMultiplayer = false;

    private GameObject _player1;
    private GameObject _player2;
    private readonly Vector2 _initialOffset = new Vector2(1f, 0f);

    /// <summary>
    /// Instancia os jogadores e ajusta o playerIndex de cada PlayerStateMachine.
    /// </summary>
    public void SpawnPlayer(Vector3 spawnPosition)
    {
        // Destrói instâncias antigas
        if (_player1 != null) { Destroy(_player1); _player1 = null; }
        if (_player2 != null) { Destroy(_player2); _player2 = null; }

        // --- PLAYER 1 ---
        if (player1Prefab != null)
        {
            _player1 = Instantiate(player1Prefab, spawnPosition, Quaternion.identity);
            _player1.tag = "Player";
            var sm1 = _player1.GetComponent<PlayerStateMachine>();
            if (sm1 != null)
                sm1.playerIndex = 1;
            else
                Debug.LogWarning("[PlayerManager] SpawnPlayer: Player1 prefab não possui PlayerStateMachine.");
        }
        else
        {
            Debug.LogError("[PlayerManager] SpawnPlayer: player1Prefab não atribuído!");
        }

        if (!isMultiplayer)
            return;

        // --- PLAYER 2 ---
        if (player2Prefab != null)
        {
            Vector3 pos2 = spawnPosition + new Vector3(_initialOffset.x, _initialOffset.y);
            _player2 = Instantiate(player2Prefab, pos2, Quaternion.identity);
            _player2.tag = "Player";
            var sm2 = _player2.GetComponent<PlayerStateMachine>();
            if (sm2 != null)
                sm2.playerIndex = 2;
            else
                Debug.LogWarning("[PlayerManager] SpawnPlayer: Player2 prefab não possui PlayerStateMachine.");
        }
        else
        {
            Debug.LogError("[PlayerManager] SpawnPlayer: player2Prefab não atribuído, mas isMultiplayer está true!");
        }
    }

    /// <summary>
    /// Destrói instâncias de jogadores existentes. Chamado pelo GameManager em CleanupPreviousRun ou similar.
    /// </summary>
    public void DestroyAllPlayers()
    {
        if (_player1 != null)
        {
            Destroy(_player1);
            _player1 = null;
        }
        if (_player2 != null)
        {
            Destroy(_player2);
            _player2 = null;
        }
    }

    #region Transição Entre Salas

    private bool _isTransitioning = false;

    public void TryMoveThroughDoor(DoorDirection dir, float dist)
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionThroughDoor(dir.ToVector(), dist));
    }

    private IEnumerator TransitionThroughDoor(Vector2Int dir, float dist)
    {
        _isTransitioning = true;
        var current = GameManager.Instance.GetCurrentRoomCoord();
        if (!GameManager.Instance.IsDoorAccessible(current, dir.ToDoorDirection()))
        {
            _isTransitioning = false;
            yield break;
        }

        var next = current + dir;
        var room = GameManager.Instance.GetRoom(next);
        if (room == null)
        {
            _isTransitioning = false;
            yield break;
        }

        Vector3 offset = new Vector3(dir.x, dir.y) * dist;
        var start1 = _player1 != null ? _player1.transform.position : Vector3.zero;
        var end1 = start1 + offset;
        Vector3 start2 = (_player2 != null) ? _player2.transform.position : Vector3.zero;
        Vector3 end2 = start2 + offset;

        float t = 0f, dur = 0.5f;
        while (t < dur)
        {
            if (_player1 != null)
                _player1.transform.position = Vector3.Lerp(start1, end1, t / dur);
            if (_player2 != null)
                _player2.transform.position = Vector3.Lerp(start2, end2, t / dur);
            t += Time.deltaTime;
            yield return null;
        }

        if (_player1 != null)
            _player1.transform.position = end1;
        if (_player2 != null)
            _player2.transform.position = end2;

        GameManager.Instance.SetCurrentRoom(next);
        _isTransitioning = false;
    }

    #endregion

    #region Encerramento de Run

    public void EndRun()
    {
        // Destrói os jogadores antes de delegar ao GameManager
        DestroyAllPlayers();
        GameManager.Instance.EndRunAndAuction();
    }

    #endregion
}
