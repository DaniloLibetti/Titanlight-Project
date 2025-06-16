// HoleTrigger.cs
using UnityEngine;
using System.Collections;
using Player.StateMachine;

[RequireComponent(typeof(Collider2D))]
public class HoleTrigger : MonoBehaviour
{
    [Header("Configura��o da Queda")]
    [Tooltip("Dura��o da anima��o de encolhimento antes do blink.")]
    public float shrinkDuration = 1.0f;
    [Tooltip("Dura��o da invencibilidade NO BURACO (antes do teleport).")]
    public float invincibleDuration = 0.5f;
    [Tooltip("Intervalo de piscada durante a invencibilidade.")]
    public float blinkInterval = 0.1f;
    [Tooltip("Dano aplicado ao cair.")]
    public float damage = 20f;

    [Header("Layer Mask para detectar buracos")]
    [Tooltip("Layer(s) onde est�o objetos de buraco. Usado para checar se a posi��o de retorno cai em outro buraco.")]
    public LayerMask holeLayerMask;

    private bool isFalling = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isFalling) return;
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<PlayerStateMachine>();
        if (player != null)
        {
            // Inicia a queda sempre que o jogador entrar no trigger do buraco,
            // independentemente de estar dash ou n�o: o dash salva LastDashPosition no in�cio.
            StartCoroutine(FallSequence(player));
            SoundManager.PlaySound(SoundType.FALL);
        }
    }

    private IEnumerator FallSequence(PlayerStateMachine player)
    {
        isFalling = true;

        // 1) trava controle e configura trigger nos colliders
        player.SetCanMove(false);
        player.SetCanDash(false);
        player.SetCollidersTrigger(true);

        var rb = player.rb;
        var origConstraints = rb.constraints;
        var origGravity = rb.gravityScale;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // guarda estado inicial para anima��es
        Vector3 fallPos = player.transform.position;
        Vector3 origScale = player.transform.localScale;

        // 2) anima encolhimento
        float t = 0f;
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(1f, 0f, t / shrinkDuration);
            player.transform.localScale = origScale * s;
            player.transform.position = fallPos; // mant�m posi��o durante encolhimento
            yield return null;
        }

        // 3) aplica dano e checa morte
        var health = player.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            if (health.CurrentHealth <= 0f)
            {
                // encerra run e sai da coroutine
                GameManager.Instance.EndRunAndAuction();
                yield break;
            }
        }

        // 4) piscada invenc�vel antes do teleport
        float blinkTimer = 0f;
        // Tenta pegar SpriteRenderer: pode estar no pr�prio objeto ou em filho
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = player.GetComponentInChildren<SpriteRenderer>();

        while (blinkTimer < invincibleDuration)
        {
            if (sr != null)
                sr.enabled = !sr.enabled;
            player.transform.position = fallPos; // fixa posi��o para n�o deslizar
            yield return new WaitForSeconds(blinkInterval);
            blinkTimer += blinkInterval;
        }
        if (sr != null)
            sr.enabled = true;

        // 5) teleport para �ltimo dash salvo
        Vector3 exitPos = player.LastDashPosition;
        bool usedFallback = false;

        // Se LastDashPosition for zero-vetor, possivelmente nunca dashou: use fallback
        if (exitPos == Vector3.zero)
        {
            Debug.LogWarning("[HoleTrigger] LastDashPosition n�o definido (zero). Usando fallback de spawn da sala.");
            usedFallback = true;
        }
        else
        {
            // Checa se exitPos cai em outro buraco: usa Physics2D.OverlapPoint com holeLayerMask
            Collider2D hit = Physics2D.OverlapPoint(exitPos, holeLayerMask);
            if (hit != null)
            {
                Debug.LogWarning("[HoleTrigger] LastDashPosition cai dentro de outro buraco. Usando fallback de spawn da sala.");
                usedFallback = true;
            }
        }

        if (usedFallback)
        {
            // Fallback: usar spawn point da sala atual
            Vector2Int roomCoord = GameManager.Instance.GetCurrentRoomCoord();
            Room currentRoom = GameManager.Instance.GetRoom(roomCoord);
            if (currentRoom != null)
            {
                Vector3 spawn = currentRoom.GetPlayerSpawnPoint();
                spawn.z = player.transform.position.z;
                exitPos = spawn;
            }
            else
            {
                // Se n�o encontrar sala, apenas mant�m posi��o original de queda
                exitPos = fallPos;
                Debug.LogWarning("[HoleTrigger] Falha ao determinar spawn da sala atual; mantendo posi��o de queda.");
            }
        }

        // Aplica teleport
        player.transform.position = exitPos;
        player.transform.localScale = origScale;

        // 6) restaura f�sica e controle
        rb.gravityScale = origGravity;
        rb.constraints = origConstraints;
        player.SetCollidersTrigger(false);
        player.SetCanMove(true);
        player.SetCanDash(true);

        isFalling = false;
    }
}
