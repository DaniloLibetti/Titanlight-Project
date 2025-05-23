using UnityEngine;
using System.Collections;
using Player.StateMachine;  // pra usar SetCanMove e SetCollidersTrigger

/// <summary>
/// Detecta quando o player fica sobre um buraco e faz:
/// 1) encolhe no lugar (sem física)  
/// 2) pisca invencível lá  
/// 3) teleporta de volta  
/// 4) devolve controle  
/// </summary>
public class HoleTrigger : MonoBehaviour
{
    [Header("Configuração da Queda")]
    [Tooltip("Duração da animação de encolhimento antes do blink.")]
    public float shrinkDuration = 1.0f;

    [Tooltip("Duração da invencibilidade NO BURACO (antes do teleport).")]
    public float invincibleDuration = 0.5f;

    [Tooltip("Intervalo de piscada durante a invencibilidade.")]
    public float blinkInterval = 0.1f;

    [Tooltip("Dano aplicado ao cair.")]
    public float damage = 20f;

    private bool isFalling = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isFalling) return;

        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerStateMachine>();
            if (player != null && !player.IsDashing)
                StartCoroutine(FallSequence(player));
            SoundManager.PlaySound(SoundType.FALL);
        }
    }

    private IEnumerator FallSequence(PlayerStateMachine player)
    {
        isFalling = true;

        // 1) trava controle, vira trigger e congela TUDO na física
        player.SetCanMove(false);
        player.SetCollidersTrigger(true);

        var rb = player.rb;
        var originalConstraints = rb.constraints;
        var originalGravity = rb.gravityScale;

        // desliga gravidade e congela posição/rotação
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // guarda a posição exata do buraco
        Vector3 fallPos = player.transform.position;
        Vector3 originalScale = player.transform.localScale;

        // 2) anima encolhimento NO MESMO LUGAR
        float timer = 0f;
        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Lerp(1f, 0f, timer / shrinkDuration);
            player.transform.localScale = originalScale * t;
            player.transform.position = fallPos;
            yield return null;
        }

        // aplica dano
        var health = player.GetComponent<Health>();
        if (health != null) health.TakeDamage(damage);

        // 3) piscada invencível, ainda preso lá
        float blinkTimer = 0f;
        var sr = player.GetComponent<SpriteRenderer>()
                 ?? player.GetComponentInChildren<SpriteRenderer>();
        while (blinkTimer < invincibleDuration)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            player.transform.position = fallPos;
            yield return new WaitForSeconds(blinkInterval);
            blinkTimer += blinkInterval;
        }
        if (sr != null) sr.enabled = true;

        // 4) agora sim, teleporta pro ponto de dash e restaura escala
        Vector3 exitPoint = player.LastDashPosition;
        if (exitPoint != Vector3.zero)
            player.transform.position = exitPoint;
        else
            Debug.LogWarning("LastDashPosition não foi definida!");

        player.transform.localScale = originalScale;

        // 5) volta física, colisão e controle
        rb.gravityScale = originalGravity;
        rb.constraints = originalConstraints;
        player.SetCollidersTrigger(false);
        player.SetCanMove(true);

        isFalling = false;
    }
}
