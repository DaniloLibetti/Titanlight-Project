using UnityEngine;
using System.Collections;
using Player.StateMachine;
using DungeonSystem;

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("Player Prefabs")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    [Header("Multiplayer Setting")]
    [Tooltip("Defines if the game is in multiplayer mode")]
    public bool isMultiplayer = false;

    public GameObject Player1 { get; private set; }
    public GameObject Player2 { get; private set; }
    private readonly Vector2 _initialOffset = new Vector2(1.5f, 0f);
    private Vector3 _lastSpawnPosition;
    private bool _isSpawning = false;
    private bool _playersAreProtected = false;
    private bool _isRespawning = false;

    private const int MAX_SPAWN_ATTEMPTS = 5;
    private const float POSITION_ADJUSTMENT = 0.3f;
    private const float PROTECTION_DURATION = 2f;

    private void Start()
    {
        Debug.Log("[PlayerManager] Start initialized");
        UpdateMultiplayerMode();
    }

    public void UpdateMultiplayerMode()
    {
        if (GameManager.Instance != null)
        {
            isMultiplayer = GameManager.Instance.IsMultiplayer;
            Debug.Log($"[PlayerManager] Multiplayer mode updated: {isMultiplayer}");
        }
    }

    public void SpawnPlayers(Vector3 spawnPosition)
    {
        if (_isSpawning || _isRespawning)
        {
            Debug.LogWarning($"[PlayerManager] Blocked recursive spawn attempt");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[PlayerManager] GameManager not found!");
            return;
        }

        try
        {
            _isSpawning = true;
            _playersAreProtected = true;
            _lastSpawnPosition = spawnPosition;

            Debug.Log($"[PlayerManager] === SPAWN PROCESS STARTED ===");
            Debug.Log($"[PlayerManager] Spawning at: {spawnPosition}");

            DestroyAllPlayers(true);

            SpawnPlayer1(spawnPosition);

            if (isMultiplayer)
            {
                SpawnPlayer2(spawnPosition);
            }
            else
            {
                Debug.Log("[PlayerManager] Skipping Player2 spawn (singleplayer mode)");
            }

            Debug.Log($"[PlayerManager] === SPAWN PROCESS COMPLETE ===");
        }
        finally
        {
            _isSpawning = false;
        }

        StartCoroutine(DisablePlayerProtectionAfterDelay());
    }

    private IEnumerator DisablePlayerProtectionAfterDelay()
    {
        Debug.Log($"[PlayerManager] Player protection active for {PROTECTION_DURATION} seconds");
        yield return new WaitForSeconds(PROTECTION_DURATION);
        _playersAreProtected = false;
        Debug.Log("[PlayerManager] Player protection disabled");
    }

    private void SpawnPlayer1(Vector3 position)
    {
        if (player1Prefab == null)
        {
            Debug.LogError("[PlayerManager] Player1 prefab missing!");
            return;
        }

        Player1 = Instantiate(player1Prefab, position, Quaternion.identity);
        Player1.tag = "player 1";
        Debug.Log($"[PlayerManager] Player1 spawned at {position}");

        AddDestructionProtection(Player1);
        SetupPlayerStateMachine(Player1, 1);
        GameManager.Instance.RegisterPlayer(1, true);
    }

    private void SpawnPlayer2(Vector3 basePosition)
    {
        if (player2Prefab == null)
        {
            Debug.LogError("[PlayerManager] Player2 prefab missing!");
            return;
        }

        Vector3 player2Position = FindSafeSpawnPosition(basePosition);
        Player2 = Instantiate(player2Prefab, player2Position, Quaternion.identity);
        Player2.tag = "player 2";
        Debug.Log($"[PlayerManager] Player2 spawned at SAFE POSITION: {player2Position}");

        AddDestructionProtection(Player2);
        SetupPlayerStateMachine(Player2, 2);
        GameManager.Instance.RegisterPlayer(2, true);
    }

    private void AddDestructionProtection(GameObject player)
    {
        var protection = player.AddComponent<PlayerDestructionProtector>();
        protection.Initialize(this);
        Debug.Log($"[PlayerManager] Added destruction protection to {player.name}");
    }

    private Vector3 FindSafeSpawnPosition(Vector3 basePosition)
    {
        Vector3 spawnPosition = basePosition + new Vector3(_initialOffset.x, _initialOffset.y, 0f);
        int attempts = 0;
        bool positionValid = false;

        Debug.Log($"[PlayerManager] Finding safe position for Player2. Start: {spawnPosition}");

        while (!positionValid && attempts < MAX_SPAWN_ATTEMPTS)
        {
            positionValid = IsPositionInRoomBounds(spawnPosition);

            if (!positionValid)
            {
                Debug.LogWarning($"[PlayerManager] Position {spawnPosition} out of bounds! Adjusting...");
                spawnPosition.x += (attempts % 2 == 0) ? -POSITION_ADJUSTMENT : POSITION_ADJUSTMENT;
                attempts++;
            }
        }

        if (attempts >= MAX_SPAWN_ATTEMPTS)
        {
            Debug.LogError($"[PlayerManager] Could not find safe position! Using base position: {basePosition}");
            return basePosition;
        }

        Debug.Log($"[PlayerManager] Found valid position after {attempts} attempts: {spawnPosition}");
        return spawnPosition;
    }

    private bool IsPositionInRoomBounds(Vector3 position)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[PlayerManager] GameManager not available. Skipping position check.");
            return true;
        }

        var currentRoom = GameManager.Instance.GetRoom(GameManager.Instance.GetCurrentRoomCoord());
        if (currentRoom == null)
        {
            Debug.LogWarning("[PlayerManager] Current room not found. Skipping position check.");
            return true;
        }

        Vector3 roomCenter = currentRoom.transform.position;
        float roomHalfWidth = GameManager.Instance.roomWidth / 2f - 1.2f;
        float roomHalfHeight = GameManager.Instance.roomHeight / 2f - 1.2f;

        bool withinX = position.x >= (roomCenter.x - roomHalfWidth) &&
                       position.x <= (roomCenter.x + roomHalfWidth);

        bool withinY = position.y >= (roomCenter.y - roomHalfHeight) &&
                       position.y <= (roomCenter.y + roomHalfHeight);

        return withinX && withinY;
    }

    private void SetupPlayerStateMachine(GameObject playerObj, int playerIndex)
    {
        PlayerStateMachine sm = playerObj.GetComponent<PlayerStateMachine>();
        if (sm != null)
        {
            sm.playerIndex = playerIndex;
            Debug.Log($"[PlayerManager] Player{playerIndex} StateMachine configured");
        }
        else
        {
            Debug.LogWarning($"[PlayerManager] Player{playerIndex} missing StateMachine component");
        }
    }

    public void DestroyAllPlayers(bool forceDestroy = false)
    {
        Debug.Log($"[PlayerManager] DestroyAllPlayers (force: {forceDestroy})");

        if (Player1 != null)
        {
            if (forceDestroy || (!_isSpawning && !_playersAreProtected))
            {
                Debug.Log($"[PlayerManager] Destroying Player1");
                var protector = Player1.GetComponent<PlayerDestructionProtector>();
                if (protector != null) protector.AllowDestruction();
                Destroy(Player1);
                Player1 = null;
            }
            else
            {
                Debug.Log($"[PlayerManager] Prevented Player1 destruction");
            }
        }

        if (Player2 != null)
        {
            if (forceDestroy || (!_isSpawning && !_playersAreProtected))
            {
                Debug.Log($"[PlayerManager] Destroying Player2");
                var protector = Player2.GetComponent<PlayerDestructionProtector>();
                if (protector != null) protector.AllowDestruction();
                Destroy(Player2);
                Player2 = null;
            }
            else
            {
                Debug.Log($"[PlayerManager] Prevented Player2 destruction");
            }
        }
    }

    public void SafeDestroyPlayer(GameObject player)
    {
        if (player == null) return;

        if (player == Player1)
        {
            Debug.Log($"[PlayerManager] SafeDestroyPlayer: Player1");
            Destroy(Player1);
            Player1 = null;
        }
        else if (player == Player2)
        {
            Debug.Log($"[PlayerManager] SafeDestroyPlayer: Player2");
            Destroy(Player2);
            Player2 = null;
        }
    }

    public void ReSpawnPlayers()
    {
        if (_isRespawning)
        {
            Debug.LogWarning("[PlayerManager] Respawn already in progress");
            return;
        }

        StartCoroutine(SafeRespawnRoutine());
    }

    private IEnumerator SafeRespawnRoutine()
    {
        _isRespawning = true;
        Debug.LogWarning("[PlayerManager] Starting safe respawn routine");
        yield return new WaitForEndOfFrame();

        try
        {
            SpawnPlayers(_lastSpawnPosition);
        }
        finally
        {
            _isRespawning = false;
        }
    }

    #region Room Transition
    private bool _isTransitioning = false;

    public void TryMoveThroughDoor(DoorDirection dir, float dist)
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionThroughDoor(dir, dist));
    }

    private IEnumerator TransitionThroughDoor(DoorDirection dir, float dist)
    {
        Debug.Log("[PlayerManager] Starting room transition");
        _isTransitioning = true;
        Vector2Int current = GameManager.Instance.GetCurrentRoomCoord();

        if (!GameManager.Instance.IsDoorAccessible(current, dir))
        {
            Debug.LogWarning("[PlayerManager] Door not accessible!");
            _isTransitioning = false;
            yield break;
        }

        Vector2Int next = current + DoorDirectionToVector2Int(dir);
        Room room = GameManager.Instance.GetRoom(next);
        if (room == null)
        {
            Debug.LogError("[PlayerManager] Next room not found!");
            _isTransitioning = false;
            yield break;
        }

        Debug.Log($"[PlayerManager] Moving to room: {next}");
        Vector3 offset = DoorDirectionToVector3(dir) * dist;

        float t = 0f, dur = 0.5f;
        while (t < dur)
        {
            if (Player1 != null) Player1.transform.position += offset * Time.deltaTime / dur;
            if (Player2 != null) Player2.transform.position += offset * Time.deltaTime / dur;
            t += Time.deltaTime;
            yield return null;
        }

        GameManager.Instance.SetCurrentRoom(next, dir.GetOpposite());
        _isTransitioning = false;
        Debug.Log("[PlayerManager] Room transition complete");
    }

    private Vector2Int DoorDirectionToVector2Int(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.Up: return Vector2Int.up;
            case DoorDirection.Right: return Vector2Int.right;
            case DoorDirection.Down: return Vector2Int.down;
            case DoorDirection.Left: return Vector2Int.left;
            default: return Vector2Int.zero;
        }
    }

    private Vector3 DoorDirectionToVector3(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.Up: return Vector3.up;
            case DoorDirection.Right: return Vector3.right;
            case DoorDirection.Down: return Vector3.down;
            case DoorDirection.Left: return Vector3.left;
            default: return Vector3.zero;
        }
    }
    #endregion

    #region Run Management
    public void EndRun()
    {
        Debug.Log("[PlayerManager] EndRun called");
        DestroyAllPlayers(true);
        GameManager.Instance.EndRun();
    }
    #endregion

    #region Debug Tools
    private void OnDrawGizmosSelected()
    {
        if (Player1 != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(Player1.transform.position, 0.3f);
        }

        if (Player2 != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Player2.transform.position, 0.3f);
        }
    }
    #endregion

    public void MoveAllPlayers(Vector3 position)
    {
        if (Player1 != null) Player1.transform.position = position;
        if (Player2 != null) Player2.transform.position = position;
    }

    // Método público para registrar jogadores (resolvendo o erro de acesso)
    public void RegisterPlayer(GameObject player, int playerIndex)
    {
        if (playerIndex == 1)
        {
            Player1 = player;
        }
        else if (playerIndex == 2)
        {
            Player2 = player;
        }
        Debug.Log($"[PlayerManager] Player {playerIndex} registered: {player.name}");
    }
}

public static class DoorDirectionExtensions
{
    public static DoorDirection GetOpposite(this DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.Up: return DoorDirection.Down;
            case DoorDirection.Right: return DoorDirection.Left;
            case DoorDirection.Down: return DoorDirection.Up;
            case DoorDirection.Left: return DoorDirection.Right;
            default: return DoorDirection.Up;
        }
    }
}