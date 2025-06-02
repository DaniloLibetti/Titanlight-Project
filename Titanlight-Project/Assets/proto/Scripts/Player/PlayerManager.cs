// PlayerManager.cs
using UnityEngine;
using System.Collections;
using Player.StateMachine;

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("Prefabs dos Jogadores")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    [Header("Configuração de Modo")]
    public bool isMultiplayer = false;

    private GameObject _player1;
    private GameObject _player2;

    private bool _player1Alive = false;
    private bool _player2Alive = false;

    private Vector2 _initialOffset = new Vector2(1f, 0f);

    public void SpawnPlayer(Vector3 spawnPosition)
    {
        // Destrói instâncias antigas
        if (_player1 != null)
        {
            Destroy(_player1);
            _player1 = null;
            _player1Alive = false;
        }
        if (_player2 != null)
        {
            Destroy(_player2);
            _player2 = null;
            _player2Alive = false;
        }

        // Spawn Players1
        if (player1Prefab == null)
        {
            Debug.LogError("[PlayerManager] player1Prefab NÃO está atribuído!");
            return;
        }

        _player1 = Instantiate(player1Prefab, spawnPosition, Quaternion.identity);
        _player1.tag = "Player";
        _player1Alive = true;
        RegisterDeathCallback(_player1, isPlayer1: true);
        DontDestroyOnLoad(_player1);

        // encerra aqui no modo singleplayer
        if (!isMultiplayer) return;

        // Spawn Player2 
        if (player2Prefab == null)
        {
            Debug.LogError("[PlayerManager] isMultiplayer = true, mas player2Prefab NÃO está atribuído!");
            return;
        }

        Vector3 spawnPos2 = spawnPosition + new Vector3(_initialOffset.x, _initialOffset.y, 0f);
        _player2 = Instantiate(player2Prefab, spawnPos2, Quaternion.identity);
        _player2.tag = "Player";
        _player2Alive = true;
        RegisterDeathCallback(_player2, isPlayer1: false);
        DontDestroyOnLoad(_player2);

        
        if (_player2.TryGetComponent<Collider2D>(out var col2D))
        {
            col2D.enabled = false;
            StartCoroutine(ReenableColliderNextFrame(col2D));
        }
    }

    private IEnumerator ReenableColliderNextFrame(Collider2D col2D)
    {
        yield return null;
        if (col2D != null)
            col2D.enabled = true;
    }

    private void RegisterDeathCallback(GameObject playerGO, bool isPlayer1)
    {
        if (playerGO.TryGetComponent<Health>(out var health))
        {
            health.onDeath.AddListener(() => OnPlayerDeath(isPlayer1));
        }
    }

    private void OnPlayerDeath(bool isPlayer1)
    {
        if (isPlayer1)
        {
            _player1Alive = false;
            if (_player1 != null) Destroy(_player1);
            Debug.Log("[PlayerManager] Jogador 1 morreu.");
        }
        else
        {
            _player2Alive = false;
            if (_player2 != null) Destroy(_player2);
            Debug.Log("[PlayerManager] Jogador 2 morreu.");
        }

        if (isMultiplayer)
        {
            if (!_player1Alive && !_player2Alive)
            {
                Debug.Log("[PlayerManager] Ambos jogadores mortos – encerrando run.");
                GameManager.Instance.EndRunAndAuction();
            }
        }
        else
        {
            Debug.Log("[PlayerManager] Jogador único morto – encerrando run.");
            GameManager.Instance.EndRunAndAuction();
        }
    }

    #region Transição Entre Salas

    private bool _isTransitioning = false;

    public void TryMoveThroughDoor(DoorDirection dir, float dist)
    {
        StartCoroutine(TransitionThroughDoor(dir.ToVector(), dist));
    }

    private IEnumerator TransitionThroughDoor(Vector2Int dir, float dist)
    {
        if (_isTransitioning) yield break;
        _isTransitioning = true;

        Vector2Int current = GameManager.Instance.GetCurrentRoomCoord();
        if (!GameManager.Instance.IsDoorAccessible(current, dir.ToDoorDirection()))
        {
            _isTransitioning = false;
            yield break;
        }

        Vector2Int next = current + dir;
        Room room = GameManager.Instance.GetRoom(next);
        if (room == null)
        {
            _isTransitioning = false;
            yield break;
        }

        // Desativa colisor dos jogadores antes da transição
        if (_player1 != null && _player1.TryGetComponent<Collider2D>(out var col1)) col1.enabled = false;
        if (_player2 != null && _player2.TryGetComponent<Collider2D>(out var col2)) col2.enabled = false;

        Vector3 offset = new Vector3(dir.x, dir.y, 0f) * dist;
        Vector3 p1Start = _player1.transform.position;
        Vector3 p2Start = _player2 != null ? _player2.transform.position : Vector3.zero;
        Vector3 p1End = p1Start + offset;
        Vector3 p2End = p2Start + offset;

        float duration = 0.5f, t = 0f;
        while (t < duration)
        {
            _player1.transform.position = Vector3.Lerp(p1Start, p1End, t / duration);
            if (_player2 != null)
                _player2.transform.position = Vector3.Lerp(p2Start, p2End, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        _player1.transform.position = p1End;
        if (_player2 != null) _player2.transform.position = p2End;

        if (_player1 != null && _player1.TryGetComponent<Collider2D>(out col1)) col1.enabled = true;
        if (_player2 != null && _player2.TryGetComponent<Collider2D>(out col2)) col2.enabled = true;

        GameManager.Instance.SetCurrentRoom(next);
        _isTransitioning = false;
    }

    #endregion

    #region Encerramento de Run

    public void EndRun()
    {
        if (_player1 != null) { Destroy(_player1); _player1 = null; _player1Alive = false; }
        if (_player2 != null) { Destroy(_player2); _player2 = null; _player2Alive = false; }
        GameManager.Instance.EndRunAndAuction();
    }

    #endregion
}
