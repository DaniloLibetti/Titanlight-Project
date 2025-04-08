using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Configura��es de Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;

    public float CurrentHealth { get; private set; }

    public UnityEvent onDeath;
    public UnityEvent<float> onDamageTaken;

    // Propriedade p�blica para acessar o valor m�ximo de vida
    public float MaxHealth
    {
        get { return maxHealth; }
    }

    void Awake()
    {
        // Inicia a vida com o valor m�ximo
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0)
            return;

        // Diminui a vida com base no dano recebido
        CurrentHealth -= damage;
        if (onDamageTaken != null)
            onDamageTaken.Invoke(damage);

        if (CurrentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        if (amount <= 0)
            return;

        // Restaura a vida sem passar do m�ximo
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    private void Die()
    {
        // Chama o evento de morte
        if (onDeath != null)
            onDeath.Invoke();

        // Destr�i o objeto se estiver marcado para isso
        if (destroyOnDeath)
            Destroy(gameObject);
    }
}
