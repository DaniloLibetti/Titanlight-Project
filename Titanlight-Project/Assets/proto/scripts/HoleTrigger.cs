using UnityEngine;
using System.Collections;
using Player.StateMachine;

[RequireComponent(typeof(Collider2D))]
public class HoleTrigger : MonoBehaviour
{
    [Header("Fall Configuration")]
    public float shrinkDuration = 1.0f;
    public float invincibleDuration = 0.5f;
    public float blinkInterval = 0.1f;
    public float damage = 20f;

    [Header("Hole Detection")]
    public LayerMask holeLayerMask; // Deve incluir a layer HoleFloor

    public bool IsFalling { get; private set; }

    private void Start()
    {
        // Garante que este objeto está na layer HoleFloor
        gameObject.layer = LayerMask.NameToLayer("HoleFloor");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsFalling) return;
        if (!other.CompareTag("Player")) return;

        // Ignora completamente jogadores na layer de dash
        if (other.gameObject.layer == LayerMask.NameToLayer("Dashing"))
            return;

        StartCoroutine(HandleFall(other.GetComponent<PlayerStateMachine>()));
    }

    public IEnumerator HandleFall(PlayerStateMachine player)
    {
        if (player == null) yield break;

        IsFalling = true;
        SoundManager.PlaySound(SoundType.FALL);

        // Desativa controle do jogador
        player.SetCanMove(false);
        player.SetCanDash(false);
        player.SetCollidersTrigger(true);

        // Congela física
        var rb = player.rb;
        var origConstraints = rb.constraints;
        var origGravity = rb.gravityScale;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Configura animação
        Vector3 fallPos = player.transform.position;
        Vector3 origScale = player.transform.localScale;

        // Animação de encolhimento
        float t = 0f;
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0.1f, t / shrinkDuration);
            player.transform.localScale = origScale * scale;
            player.transform.position = fallPos;
            yield return null;
        }

        // Aplica dano
        var health = player.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            if (health.CurrentHealth <= 0f)
            {
                // Não é mais necessário chamar EndRunAndAuction aqui
                // O componente Health já notificará o GameManager sobre a morte
                yield break;
            }
        }

        // Efeito piscante
        SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
        float blinkTimer = 0f;

        while (blinkTimer < invincibleDuration)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            blinkTimer += blinkInterval;
        }
        if (sr != null) sr.enabled = true;

        // Teleporta para posição segura
        Vector3 safePosition = GetSafePosition(player);
        player.transform.position = safePosition;
        player.transform.localScale = origScale;

        // Restaura física e controle
        rb.gravityScale = origGravity;
        rb.constraints = origConstraints;
        player.SetCollidersTrigger(false);
        player.SetCanMove(true);
        player.SetCanDash(true);

        IsFalling = false;
    }

    private Vector3 GetSafePosition(PlayerStateMachine player)
    {
        Vector3 position = player.LastDashPosition;

        // Use last dash position if valid
        if (position != Vector3.zero && !IsPositionInHole(position))
            return position;

        // Fallback to room spawn point
        Vector2Int roomCoord = GameManager.Instance.GetCurrentRoomCoord();
        Room currentRoom = GameManager.Instance.GetRoom(roomCoord);

        return currentRoom != null ?
            currentRoom.GetPlayerSpawnPoint() :
            player.transform.position;
    }

    private bool IsPositionInHole(Vector3 position)
    {
        return Physics2D.OverlapPoint(position, holeLayerMask) != null;
    }
}