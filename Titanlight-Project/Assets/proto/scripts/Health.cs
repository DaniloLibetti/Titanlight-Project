using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Configurações de Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool destroyOnDeath = true;

    public float CurrentHealth { get; private set; }

    public UnityEvent onDeath;
    public UnityEvent<float> onDamageTaken;
    public UnityEvent onDropMoeda;

    // Propriedade pública para acessar o valor máximo de vida
    public float MaxHealth
    {
        get { return maxHealth; }
    }

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0)
            return;

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

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    private void Die()
    {
        // Chama o evento de morte
        if (onDeath != null)
            onDeath.Invoke();

        if (onDropMoeda != null)
            onDropMoeda.Invoke();

        // Destrói o objeto se estiver marcado para isso
        if (destroyOnDeath)
            Destroy(gameObject);
    }
}
