using UnityEngine;

/// <summary>
/// Hitbox de dano ativada durante o dash do jogador.
/// Deve estar em um GameObject filho do Player, com CircleCollider2D marcado como isTrigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DamageHitbox : MonoBehaviour
{
    [Tooltip("Dano aplicado pelo dash")]
    public int dashDamage = 10;

    private Player.StateMachine.PlayerStateMachine player;

    void Awake()
    {
        // Busca o PlayerStateMachine no objeto pai
        player = GetComponentInParent<Player.StateMachine.PlayerStateMachine>();
        if (player == null)
            Debug.LogError("DamageHitbox: não encontrou PlayerStateMachine no parent.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Só aplica dano enquanto estiver dashando
        if (player == null || !player.IsDashing)
            return;

        // Se o alvo tiver componente Health, aplica dano
        if (other.TryGetComponent<Health>(out var h))
        {
            h.TakeDamage(dashDamage);
        }
    }
}
