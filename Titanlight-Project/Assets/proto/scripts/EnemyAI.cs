using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { Idle, Alert, Chase, Attack, Standby }

    [Header("Configuracoes Gerais")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Health health;

    [Header("Configuracoes de Stun e Feedback")]
    [SerializeField] private float stunDuration = 1f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.3f;

    [Header("Deteccao e Ataque")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float alertDuration = 0.5f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float standbyDuration = 1.0f;

    // Guarda o collider da sala
    private Collider2D roomCollider;

    private Transform player;
    private Rigidbody2D rb;
    private float previousHealth;
    private Vector2 homePosition;

    // Estado da IA e timer de alerta
    private AIState currentState = AIState.Idle;
    private float alertTimer = 0f;

    // Controle de stun e efeito visual
    private bool isStunned = false;
    private SpriteRenderer spriteRenderer;
    private Color normalColor = Color.white;
    private Color attackColor = Color.red;
    private Color damageColor = Color.red;
    private Color standbyColor = Color.yellow;

    void Start()
    {
        // Pega o player, o rigidbody e o sprite
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (health == null)
            health = GetComponent<Health>();

        previousHealth = (health != null) ? health.CurrentHealth : 0;
        homePosition = transform.position;

        // Procura o collider da sala usando a layer "floordetect"
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
        {
            Debug.LogWarning("Nenhum collider da sala encontrado");
        }
    }

    void Update()
    {
        // Se estiver stunado, nao faz nada
        if (isStunned)
            return;

        // Checa se o player esta na sala
        bool playerInRoom = roomCollider != null && roomCollider.OverlapPoint(player.position);

        switch (currentState)
        {
            case AIState.Idle:
                rb.linearVelocity = Vector2.zero;
                if (playerInRoom)
                {
                    currentState = AIState.Alert;
                    alertTimer = alertDuration;
                    Debug.Log("Player entrou na sala, alerta");
                }
                break;

            case AIState.Alert:
                rb.linearVelocity = Vector2.zero;
                alertTimer -= Time.deltaTime;
                if (alertTimer <= 0f)
                {
                    currentState = AIState.Chase;
                    Debug.Log("Comecou a perseguir");
                }
                if (!playerInRoom)
                {
                    currentState = AIState.Idle;
                    Debug.Log("Player saiu da sala no alerta");
                }
                break;

            case AIState.Chase:
                if (!playerInRoom)
                {
                    currentState = AIState.Idle;
                    Debug.Log("Player saiu da sala, volta pro idle");
                }
                else
                {
                    float distance = Vector2.Distance(transform.position, player.position);
                    if (distance <= attackRange)
                    {
                        StartCoroutine(PerformAttack());
                    }
                    else
                    {
                        Vector2 direction = (player.position - transform.position).normalized;
                        rb.linearVelocity = direction * moveSpeed;
                    }
                }
                break;

            case AIState.Attack:
                // Durante o dash, o movimento vem da corrotina
                break;

            case AIState.Standby:
                rb.linearVelocity = Vector2.zero;
                break;
        }

        // Se a vida diminuiu, chama o efeito de dano e stun
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
        // Muda a cor pra vermelho
        spriteRenderer.color = attackColor;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);

        // Faz o dash pro player
        Vector2 dashDirection = (player.position - transform.position).normalized;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = standbyColor;
        currentState = AIState.Standby;
        yield return new WaitForSeconds(standbyDuration);

        // Volta a cor normal e continua a perseguir
        spriteRenderer.color = normalColor;
        currentState = AIState.Chase;
    }

    IEnumerator FlashDamage()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = originalColor;
    }

    void OnTakeDamage()
    {
        if (player != null)
        {
            Vector2 knockbackDirection = (transform.position - player.position).normalized;
            StartCoroutine(DoStun(knockbackDirection));
        }
    }

    IEnumerator DoStun(Vector2 knockbackDirection)
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // Guarda a posicao original do sprite
        Vector3 originalSpritePos = spriteRenderer.transform.localPosition;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            // Shake no sprite so pra efeito visual
            Vector3 shakeOffset = (Vector3)Random.insideUnitCircle * shakeIntensity;
            spriteRenderer.transform.localPosition = shakeOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Volta a posicao original do sprite
        spriteRenderer.transform.localPosition = originalSpritePos;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (currentState == AIState.Attack)
            {
                if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController playerController))
                {
                    playerController.TakeDamage(damage);
                }
            }
        }
    }
}
