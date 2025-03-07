using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Health health;

    [Header("Configurações de Stun")]
    [SerializeField] private float stunDuration = 1f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.3f;

    private Transform player;
    private Rigidbody2D rb;
    private bool isStunned = false;
    private float previousHealth;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        if (health == null) health = GetComponent<Health>();
        previousHealth = health != null ? health.currentHealth : 0;
    }

    void Update()
    {
        if (!isStunned && health != null && health.currentHealth > 0)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed; // Corrigido de linearVelocity para velocity
        }

        if (health != null && health.currentHealth < previousHealth)
        {
            OnTakeDamage();
            previousHealth = health.currentHealth;
        }
    }

    void OnTakeDamage()
    {
        if (player != null)
        {
            Vector2 knockbackDirection = (transform.position - player.position).normalized;
            StartCoroutine(EfeitoStun(knockbackDirection));
        }
    }

    IEnumerator EfeitoStun(Vector2 direcaoKnockback)
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero; // Para o inimigo antes do knockback
        rb.AddForce(direcaoKnockback * knockbackForce, ForceMode2D.Impulse);

        // Efeito de tremor
        float tempo = 0f;
        Vector2 posicaoOriginal = rb.position;

        while (tempo < shakeDuration)
        {
            rb.MovePosition(posicaoOriginal + Random.insideUnitCircle * shakeIntensity);
            tempo += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController player))
            {
                player.TakeDamage(damage);
            }
        }
    }
}
