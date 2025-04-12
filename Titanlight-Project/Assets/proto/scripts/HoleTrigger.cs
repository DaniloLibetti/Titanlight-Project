using UnityEngine;
using System.Collections;

public class HoleTrigger : MonoBehaviour
{
    [Header("Configura��es da Queda")]
    [Tooltip("Dura��o da sequ�ncia de encolhimento e recupera��o da escala.")]
    public float shrinkDuration = 1.0f;

    [Tooltip("Quantidade de dano que o jogador receber� ao cair no buraco.")]
    public float damage = 20f;

    private bool isFalling = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Obt�m o PlayerController
            PlayerController player = other.GetComponent<PlayerController>();
            // Verifica se o jogador n�o est� dashing e se a sequ�ncia de queda n�o est� ocorrendo
            if (player != null && !player.IsDashing && !isFalling)
            {
                StartCoroutine(FallSequence(player));
            }
        }
    }

    private IEnumerator FallSequence(PlayerController player)
    {
        isFalling = true;

        // Desabilita o controle do jogador
        player.CanMove = false;

        // Armazena a escala original do jogador
        Vector3 originalScale = player.transform.localScale;
        float timer = 0f;

        // Sequ�ncia de encolhimento (efeito de "suc��o" para dentro do buraco)
        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            float scaleLerp = Mathf.Lerp(1f, 0f, timer / shrinkDuration);
            player.transform.localScale = originalScale * scaleLerp;
            yield return null;
        }

        // Reposiciona o jogador no ponto final do dash (�ltima posi��o registrada)
        Vector3 exitPoint = player.LastDashPosition;
        if (exitPoint != Vector3.zero)
        {
            player.transform.position = exitPoint;
        }
        else
        {
            Debug.LogWarning("LastDashPosition do jogador n�o foi definida!");
        }

        // Aplica dano se o componente Health existir
        Health health = player.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // Sequ�ncia de crescimento para restaurar a escala original
        timer = 0f;
        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            float scaleLerp = Mathf.Lerp(0f, 1f, timer / shrinkDuration);
            player.transform.localScale = originalScale * scaleLerp;
            yield return null;
        }

        // Habilita novamente o controle do jogador
        player.CanMove = true;

        isFalling = false;
    }
}
