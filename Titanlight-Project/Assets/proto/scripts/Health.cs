using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private GameObject deathEffect;

    public float currentHealth { get; private set; }

    void Start() => currentHealth = maxHealth;

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public void ResetHealth() => currentHealth = maxHealth;
}