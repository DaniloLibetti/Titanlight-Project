using UnityEngine;
using System.Collections;

public class HoleTrigger : MonoBehaviour
{
    [Header("Configurações da Queda")]
    [Tooltip("Duração da sequência de encolhimento e recuperação da escala.")]
    public float shrinkDuration = 1.0f;

    [Tooltip("Quantidade de dano que o jogador receberá ao cair no buraco.")]
    public float damage = 20f;

    private bool isFalling = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Obtém o PlayerController
            PlayerController player = other.GetComponent<PlayerController>();
            // Verifica se o jogador não está dashing e se a sequência de queda não está ocorrendo
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

        // Sequência de encolhimento (efeito de "sucção" para dentro do buraco)
        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            float scaleLerp = Mathf.Lerp(1f, 0f, timer / shrinkDuration);
            player.transform.localScale = originalScale * scaleLerp;
            yield return null;
        }

        // Reposiciona o jogador no ponto final do dash (última posição registrada)
        Vector3 exitPoint = player.LastDashPosition;
        if (exitPoint != Vector3.zero)
        {
            player.transform.position = exitPoint;
        }
        else
        {
            Debug.LogWarning("LastDashPosition do jogador não foi definida!");
        }

        // Aplica dano se o componente Health existir
        Health health = player.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // Sequência de crescimento para restaurar a escala original
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
