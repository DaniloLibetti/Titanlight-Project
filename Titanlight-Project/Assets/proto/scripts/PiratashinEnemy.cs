// PiratashinEnemy.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyCoinDrop))]
public class PiratashinEnemy : MonoBehaviour
{
    public enum AIState { Idle, Chase, Attack }

    [Header("— Componentes Obrigatórios —")]
    private Rigidbody2D rb;
    private EnemyCoinDrop coinDrop;

    [Header("Configuração Geral")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Parâmetros de Movimento")]
    public float moveSpeed = 3f;
    public float meleeRange = 1.5f;

    [Header("Ataque Normal")]
    public float normalAttackDamage = 4f;
    public float attackCooldown = 1f;
    public float normalChargeTime = 0.5f;

    [Header("Efeitos Visuais")]
    public float postAttackCooldown = 0.5f;

    [Header("Detecção da Sala")]
    [SerializeField] private Collider2D roomCollider;

    [Header("Vida do Inimigo")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;

    private SegmentedHealthBar healthBar;

    // Estados internos
    private Transform player;
    private AIState currentState = AIState.Idle;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private Color defaultColor = Color.white;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coinDrop = GetComponent<EnemyCoinDrop>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        healthBar = GetComponent<SegmentedHealthBar>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        StartCoroutine(FindPlayer());
        FindRoomCollider();
    }

    IEnumerator FindPlayer()
    {
        while (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
                player = go.transform;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void FindRoomCollider()
    {
        int layer = LayerMask.NameToLayer("floordetect");
        var cols = Physics2D.OverlapPointAll(transform.position);
        foreach (var col in cols)
        {
            if (col.gameObject.layer == layer)
            {
                roomCollider = col;
                break;
            }
        }
        if (roomCollider == null)
            Debug.LogWarning("PiratashinEnemy: collider de sala não encontrado na layer 'floordetect'");
    }

    void Update()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (player == null || roomCollider == null)
            return;

        bool playerInRoom = roomCollider.OverlapPoint(player.position);
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case AIState.Idle:
                rb.linearVelocity = Vector2.zero;
                if (playerInRoom)
                    currentState = AIState.Chase;
                break;

            case AIState.Chase:
                if (!playerInRoom)
                {
                    currentState = AIState.Idle;
                    return;
                }
                if (distToPlayer <= meleeRange && attackTimer <= 0f && !isAttacking)
                {
                    attackTimer = attackCooldown;
                    isAttacking = true;
                    currentState = AIState.Attack;
                    StartCoroutine(NormalAttack());
                    return;
                }
                Vector2 dir = (player.position - transform.position).normalized;
                rb.linearVelocity = dir * moveSpeed;
                break;

            case AIState.Attack:
                // Aguarda fim da coroutine de ataque
                break;
        }

        UpdateAnimations();
        UpdateFacingDirection();
    }

    IEnumerator NormalAttack()
    {
        // 1) Congela movimento
        var prevC = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;

        // 2) Efeito de carga
        yield return StartCoroutine(ChargeEffect(normalChargeTime));

        // 3) Restaura movimento
        rb.constraints = prevC;

        // 4) Animação e dano
        animator?.SetTrigger("Attack");
        ApplyDamageIfInRange(normalAttackDamage);

        // 5) Pós-ataque
        yield return new WaitForSeconds(postAttackCooldown);

        isAttacking = false;
        currentState = AIState.Chase;
    }

    IEnumerator ChargeEffect(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            spriteRenderer.color = Color.Lerp(defaultColor, Color.red, Mathf.PingPong(t * 5f, 1f));
            t += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = defaultColor;
    }

    void ApplyDamageIfInRange(float damage)
    {
        if (Vector2.Distance(transform.position, player.position) > meleeRange)
            return;
        if (player.TryGetComponent<PlayerController>(out var pc))
        {
            pc.TakeDamage(damage);
            //healthBar.UpdateHealthBar(normalAttackDamage);
        }
            
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        // Chama o drop público
        coinDrop?.DropItems();

        // (Opcional) partículas e sons de morte
        // Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        bool walking = rb.linearVelocity.magnitude > 0.1f;
        animator.SetBool("IsWalking", walking);
        if (walking)
        {
            animator.SetFloat("MoveX", rb.linearVelocity.x);
            animator.SetFloat("MoveY", rb.linearVelocity.y);
        }
    }

    void UpdateFacingDirection()
    {
        if (player == null) return;
        Vector2 diff = player.position - transform.position;
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            animator.SetFloat("FaceX", diff.x > 0 ? 1 : -1);
            animator.SetFloat("FaceY", 0);
        }
        else
        {
            animator.SetFloat("FaceX", 0);
            animator.SetFloat("FaceY", diff.y > 0 ? 1 : -1);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Lógica extra de colisão se precisar
    }
}
