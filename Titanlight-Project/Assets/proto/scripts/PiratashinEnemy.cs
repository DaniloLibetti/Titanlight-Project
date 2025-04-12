using UnityEngine;
using System.Collections;

public class PiratashinEnemy : MonoBehaviour
{
    public enum AIState { Idle, Chase, Attack }

    [Header("Configuração Geral")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Parâmetros de Movimento")]
    public float moveSpeed = 3f;
    public float meleeRange = 1.5f;

    [Header("Ataque Normal")]
    public float normalAttackDamage = 10f;
    public float attackCooldown = 1f;
    public float normalChargeTime = 0.5f;

    [Header("Efeitos Visuais")]
    public float postAttackCooldown = 0.5f;

    [Header("Detecção da Sala")]
    [SerializeField] private Collider2D roomCollider;

    [Header("Área de Roaming")]
    [Tooltip("Raio máximo que o inimigo pode se afastar de sua posição de origem.")]
    public float maxRoamRadius = 5f;

    // --- Campos Privados ---
    private Transform player;
    private Rigidbody2D rb;
    private RigidbodyConstraints2D defaultConstraints;
    private Vector2 homePosition;
    private AIState currentState = AIState.Idle;

    private bool isAttacking = false;
    private float attackTimer = 0f;

    private Color defaultColor = Color.white;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Assegure no Inspector que Freeze Rotation Z está marcado
        // para impedir qualquer rotação em Z
        defaultConstraints = rb.constraints;

        homePosition = transform.position;

        // Inicia busca pelo player em background
        StartCoroutine(FindPlayer());

        // Detecta o collider da sala de spawn
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
            Debug.LogWarning("PiratashinEnemy: não encontrou collider de sala em 'floordetect'");
    }

    void Update()
    {
        // Atualiza cooldown de ataque
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
                // Se saiu da sala, volta a Idle
                if (!playerInRoom)
                {
                    currentState = AIState.Idle;
                    return;
                }

                // Se dentro do alcance e cooldown liberado, inicia ataque
                if (distToPlayer <= meleeRange && attackTimer <= 0f && !isAttacking)
                {
                    attackTimer = attackCooldown;
                    isAttacking = true;
                    currentState = AIState.Attack;
                    StartCoroutine(NormalAttack());
                    return;
                }

                // Movimento de perseguição
                Vector2 dir = (player.position - transform.position).normalized;
                rb.linearVelocity = dir * moveSpeed;

                break;

            case AIState.Attack:
                // aguardando coroutine liberar
                break;
        }

        UpdateAnimations();
        UpdateFacingDirection();
    }

    IEnumerator NormalAttack()
    {
        // Congela posição e rotação
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;

        // Efeito de carga
        yield return StartCoroutine(ChargeEffect(normalChargeTime));

        // Restaura constraints originais (posição X/Y livre, rotação Z ainda congelada)
        rb.constraints = defaultConstraints;

        // Dispara animação e aplica dano
        animator?.SetTrigger("Attack");
        ApplyDamageIfInRange(normalAttackDamage);

        // Pausa pós-ataque
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
            pc.TakeDamage(damage);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Nenhuma lógica de dash ativa por enquanto
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
}
