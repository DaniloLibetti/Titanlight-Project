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

    public float MaxHealth => maxHealth;

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0) return;

        CurrentHealth -= damage;
        onDamageTaken?.Invoke(damage);

        if (CurrentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        if (amount <= 0) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    private void Die()
    {
        onDeath?.Invoke();
        onDropMoeda?.Invoke();

        if (destroyOnDeath) Destroy(gameObject);
    }
}