using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { Idle, Alert, Chase, Attack, Standby }

    [Header("Configurações Gerais")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Health health;

    [Header("Configurações de Stun e Feedback")]
    [SerializeField] private float stunDuration = 1f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.3f;

    [Header("Detecção e Ataque")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float alertDuration = 0.5f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float standbyDuration = 1.0f;

    private Collider2D roomCollider;
    private Transform player;
    private Rigidbody2D rb;
    private float previousHealth;
    private Vector2 homePosition;
    private AIState currentState = AIState.Idle;
    private float alertTimer = 0f;
    private bool isStunned = false;
    private SpriteRenderer spriteRenderer;
    private Color normalColor = Color.white;
    private Color attackColor = Color.red;
    private Color damageColor = Color.red;
    private Color standbyColor = Color.yellow;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (health == null)
            health = GetComponent<Health>();

        if (health != null)
        {
            health.onDeath.AddListener(OnDeath);
            previousHealth = health.CurrentHealth;
        }

        homePosition = transform.position;

        int floorDetectLayer = LayerMask.NameToLayer("floordetect");
        Collider2D[] colliders = Physics2D.OverlapPointAll(transform.position);
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject.layer == floorDetectLayer)
            {
                roomCollider = col;
                break;
            }
        }
        if (roomCollider == null)
            Debug.LogWarning("Nenhum collider da sala encontrado");
    }

    void Update()
    {
        if (isStunned) return;

        bool playerInRoom = roomCollider != null && roomCollider.OverlapPoint(player.position);

        switch (currentState)
        {
            case AIState.Idle:
                rb.linearVelocity = Vector2.zero;
                if (playerInRoom)
                {
                    currentState = AIState.Alert;
                    alertTimer = alertDuration;
                }
                break;

            case AIState.Alert:
                rb.linearVelocity = Vector2.zero;
                alertTimer -= Time.deltaTime;
                if (alertTimer <= 0f)
                    currentState = AIState.Chase;
                else if (!playerInRoom)
                    currentState = AIState.Idle;
                break;

            case AIState.Chase:
                if (!playerInRoom)
                {
                    currentState = AIState.Idle;
                }
                else
                {
                    float dist = Vector2.Distance(transform.position, player.position);
                    if (dist <= attackRange)
                        StartCoroutine(PerformAttack());
                    else
                    {
                        Vector2 dir = (player.position - transform.position).normalized;
                        rb.linearVelocity = dir * moveSpeed;
                    }
                }
                break;

            case AIState.Attack:
                // nada aqui, a coroutine faz o trabalho
                break;

            case AIState.Standby:
                rb.linearVelocity = Vector2.zero;
                break;
        }

        if (health != null && health.CurrentHealth < previousHealth)
        {
            StartCoroutine(FlashDamage());
            OnTakeDamage();
            previousHealth = health.CurrentHealth;
        }
    }

    IEnumerator PerformAttack()
    {
        currentState = AIState.Attack;
        spriteRenderer.color = attackColor;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);

        Vector2 dashDir = (player.position - transform.position).normalized;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = standbyColor;
        currentState = AIState.Standby;
        yield return new WaitForSeconds(standbyDuration);

        spriteRenderer.color = normalColor;
        currentState = AIState.Chase;
    }

    IEnumerator FlashDamage()
    {
        Color orig = spriteRenderer.color;
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = orig;
    }

    void OnTakeDamage()
    {
        if (player == null) return;
        Vector2 knockDir = (transform.position - player.position).normalized;
        StartCoroutine(DoStun(knockDir));
    }

    IEnumerator DoStun(Vector2 knockDir)
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);

        Vector3 origPos = spriteRenderer.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            Vector3 shake = (Vector3)Random.insideUnitCircle * shakeIntensity;
            spriteRenderer.transform.localPosition = shake;
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.transform.localPosition = origPos;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player") && currentState == AIState.Attack)
        {
            if (col.gameObject.TryGetComponent<PlayerController>(out var pc))
                pc.TakeDamage(damage);
        }
    }

    private void OnDeath()
    {
        // Apenas destrói o inimigo: o drop de moedas
        // fica por conta do EnemyCoinDrop
        Destroy(gameObject);
    }
}
