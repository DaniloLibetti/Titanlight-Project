using UnityEngine;
using System.Collections;
using Player.StateMachine;

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("Prefabs dos Jogadores")]
    [Tooltip("Prefab para o Jogador 1")]
    public GameObject player1Prefab;

    [Tooltip("Prefab para o Jogador 2 (somente em modo multiplayer)")]
    public GameObject player2Prefab;

    [Header("Configuração de Modo")]
    [Tooltip("Se verdadeiro, instanciará dois jogadores; caso contrário, apenas um")]
    public bool isMultiplayer = false;

    // Referências internas aos GameObjects instanciados
    private GameObject _player1;
    private GameObject _player2;

    // Offset inicial para Player 2 (X positivo ao lado do P1)
    private Vector2 _initialOffset = new Vector2(3f, 0f);

    #region Instanciação e Morte dos Jogadores

    public void SpawnPlayer(Vector3 spawnPosition)
    {
        Debug.Log($"[PlayerManager] SpawnPlayer chamado. isMultiplayer = {isMultiplayer}");

        // --- Jogador 1 ---
        if (_player1 != null)
        {
            Destroy(_player1);
            _player1 = null;
        }

        if (player1Prefab == null)
        {
            Debug.LogError("[PlayerManager] player1Prefab NÃO está atribuído no Inspector!");
            return;
        }

        _player1 = Instantiate(player1Prefab, spawnPosition, Quaternion.identity);
        RegisterDeathCallback(_player1);
        DontDestroyOnLoad(_player1);
        Debug.Log($"[PlayerManager] Jogador 1 instanciado em {spawnPosition}");

        // --- Jogador 2 (opcional) ---
        if (!isMultiplayer)
        {
            if (_player2 != null)
            {
                Destroy(_player2);
                _player2 = null;
                Debug.Log("[PlayerManager] isMultiplayer = false → destruiu eventual Jogador 2 antigo.");
            }
            return;
        }

        if (player2Prefab == null)
        {
            Debug.LogError("[PlayerManager] isMultiplayer = true, mas player2Prefab NÃO está atribuído!");
            return;
        }

        // Destrói instância anterior
        if (_player2 != null)
        {
            Destroy(_player2);
            _player2 = null;
        }

        // Posiciona Player2 ao lado do Player1
        Vector3 spawnPos2 = spawnPosition + new Vector3(_initialOffset.x, _initialOffset.y, 0f);
        _player2 = Instantiate(player2Prefab, spawnPos2, Quaternion.identity);
        RegisterDeathCallback(_player2);
        DontDestroyOnLoad(_player2);
        Debug.Log($"[PlayerManager] Jogador 2 instanciado em {spawnPos2}");

        // Protege contra colisões imediatas
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

    private void RegisterDeathCallback(GameObject playerGO)
    {
        if (playerGO.TryGetComponent<Health>(out var health))
        {
            health.onDeath.AddListener(OnPlayerDeath);
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("[PlayerManager] OnPlayerDeath chamado – destruindo jogadores e encerrando run.");
        if (_player1 != null) Destroy(_player1);
        if (_player2 != null) Destroy(_player2);
        _player1 = _player2 = null;
        GameManager.Instance.EndRunAndAuction();
    }

    #endregion

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

        // Desativa colisor de ambos jogadores
        foreach (var go in new[] { _player1, _player2 })
        {
            if (go != null && go.TryGetComponent<Collider2D>(out var c))
                c.enabled = false;
        }

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

        foreach (var go in new[] { _player1, _player2 })
        {
            if (go != null && go.TryGetComponent<Collider2D>(out var c))
                c.enabled = true;
        }

        GameManager.Instance.SetCurrentRoom(next);
        _isTransitioning = false;
    }

    #endregion

    #region Encerramento de Run

    public void EndRun()
    {
        if (_player1 != null) Destroy(_player1);
        if (_player2 != null) Destroy(_player2);
        _player1 = _player2 = null;
        GameManager.Instance.EndRunAndAuction();
    }

    #endregion
}
