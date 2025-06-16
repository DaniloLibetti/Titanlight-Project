using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour
{
    [Header("Configurações de Vida")]
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("Se verdadeiro, destrói o GameObject imediatamente em Die(). Caso precise tocar animação, use false e destrua manualmente após animação.")]
    [SerializeField] private bool destroyOnDeath = false;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;

    private bool isDead = false;

    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged; // (current, max)

    [Header("Eventos (UnityEvent)")]
    public UnityEvent onDeath;
    public UnityEvent<float> onDamageTaken;
    public UnityEvent<float, float> onHealthChangedUnity;
    public UnityEvent onDropMoeda;

    void Awake()
    {
        CurrentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        if (damage <= 0f) return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);

        onDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        onHealthChangedUnity?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        if (amount <= 0f) return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        onHealthChangedUnity?.Invoke(CurrentHealth, maxHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();
        onDeath?.Invoke();
        onDropMoeda?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject);
    }

    public void DestroyAfterDeathAnimation()
    {
        if (isDead && gameObject != null)
            Destroy(gameObject);
    }

    public void ResetHealth()
    {
        isDead = false;
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        onHealthChangedUnity?.Invoke(CurrentHealth, maxHealth);
    }
}
