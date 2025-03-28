using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private GameObject deathEffect;

    private float currentHealth;

    private void Start() => currentHealth = 1;

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    public void AddHealth(int healthBoost)
    {
        int health = Mathf.RoundToInt(currentHealth * maxHealth);
        int val = health + healthBoost;
        currentHealth = (val > maxHealth ? maxHealth : val / maxHealth);
    }

    private void Die()
    {
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}