using UnityEngine;
using System.Collections;

public class PiratashinEnemy : MonoBehaviour
{
    [Header("Configuração Geral")]
    public Transform player;
    public Animator animator;
    public SpriteRenderer spriteRenderer; // Arraste o SpriteRenderer pelo Inspector

    [Header("Parâmetros de Movimento")]
    public float moveSpeed = 3f;
    public float meleeRange = 1.5f;

    [Header("Ataque Normal")]
    public float normalAttackDamage = 10f;
    public float attackCooldown = 1f;
    public float normalChargeTime = 0.5f; // Tempo de carga com efeito de piscar

    [Header("Ataque Dash (Investida)")]
    public float dashChargeTime = 2f;      // Tempo para carregar a investida
    public float dashDuration = 0.3f;        // Duração do dash
    public float investidaDashSpeed = 15f;   // Velocidade do dash

    [Header("Efeitos Visuais")]
    public float postAttackCooldown = 0.5f;  // Tempo de pausa pós-ataque

    private Rigidbody2D rb;
    private Color defaultColor = Color.white;
    private bool isAttacking = false;
    private float dashTimer = 0f;          // Acumula tempo longe do player
    private float attackTimer = 0f;        // Cooldown pro ataque normal
    private bool isDashing = false;        // Indica se tá na fase de dash

    private RigidbodyConstraints2D defaultConstraints; // Pra salvar constraints iniciais

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultConstraints = rb.constraints; // Armazena os constraints padrões
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null)
            return;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (!isAttacking)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= meleeRange)
            {
                dashTimer = 0f; // Reseta o timer se estiver perto do player
                rb.linearVelocity = Vector2.zero;

                if (attackTimer <= 0f)
                {
                    StartCoroutine(NormalAttack());
                    attackTimer = attackCooldown;
                }
            }
            else
            {
                // Se estiver longe, acumula tempo
                dashTimer += Time.deltaTime;
                if (dashTimer >= 2f)
                {
                    StartCoroutine(DashAttack());
                    dashTimer = 0f;
                }
                else
                {
                    // Continua se movendo em direção ao player
                    Vector2 direction = (player.position - transform.position).normalized;
                    rb.linearVelocity = direction * moveSpeed;
                }
            }
        }

        UpdateAnimations();
        UpdateFacingDirection();
    }

    IEnumerator NormalAttack()
    {
        isAttacking = true;

        // Congela o inimigo durante a carga
        RigidbodyConstraints2D originalConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;

        // Efeito de carga: pisca entre branco e vermelho
        yield return StartCoroutine(ChargeEffect(normalChargeTime));

        // Restaura os constraints para permitir movimentação
        rb.constraints = originalConstraints;

        // Aciona a animação de ataque normal
        if (animator != null)
            animator.SetTrigger("Attack");

        // Delay para sincronizar com a animação
        yield return new WaitForSeconds(0.2f);
        ApplyDamageIfInRange();
        yield return new WaitForSeconds(postAttackCooldown);
        isAttacking = false;
    }

    IEnumerator DashAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        // Congela o inimigo enquanto carrega o dash
        RigidbodyConstraints2D originalConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;

        float chargeTime = dashChargeTime;
        while (chargeTime > 0f)
        {
            // Durante o dash charge, o inimigo fica parado
            chargeTime -= Time.deltaTime;
            yield return null;
        }

        // Restaura os constraints para permitir o dash
        rb.constraints = originalConstraints;

        // Registra a posição do player no início do dash
        Vector2 dashTarget = player.position;
        spriteRenderer.color = Color.blue;
        isDashing = true;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            Vector2 direction = (dashTarget - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * investidaDashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = defaultColor;
        isDashing = false;

        // O dano do dash será aplicado somente se houver colisão com o player

        yield return new WaitForSeconds(postAttackCooldown);
        isAttacking = false;
    }

    // Coroutine que faz o sprite piscar entre branco e vermelho durante o tempo de carga
    IEnumerator ChargeEffect(float chargeTime)
    {
        float timer = 0f;
        while (timer < chargeTime)
        {
            float t = Mathf.PingPong(timer * 5, 1f);
            spriteRenderer.color = Color.Lerp(defaultColor, Color.red, t);
            timer += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = defaultColor;
    }

    // Aplica dano apenas se o player estiver próximo
    void ApplyDamageIfInRange()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= meleeRange)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                Debug.Log("Aplicando dano de " + normalAttackDamage);
                pc.TakeDamage(normalAttackDamage);
            }
            else
            {
                Debug.LogWarning("PlayerController não encontrado no player!");
            }
        }
        else
        {
            Debug.Log("Player fora de alcance. Dano não aplicado.");
        }
    }

    // Se colidir com o player durante o dash, aplica dano
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing && collision.gameObject.CompareTag("Player"))
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                Debug.Log("Dash: Aplicando dano de " + normalAttackDamage);
                pc.TakeDamage(normalAttackDamage);
            }
            isDashing = false;
        }
    }

    void UpdateAnimations()
    {
        if (animator == null)
            return;

        Vector2 velocity = rb.linearVelocity;
        bool isWalking = velocity.magnitude > 0.1f;
        animator.SetBool("IsWalking", isWalking);
        if (isWalking)
        {
            animator.SetFloat("MoveX", velocity.x);
            animator.SetFloat("MoveY", velocity.y);
        }
    }

    // Atualiza a direção que o inimigo "olha" com base na posição do player
    void UpdateFacingDirection()
    {
        Vector2 diff = player.position - transform.position;
        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            if (diff.x > 0)
            {
                animator.SetFloat("FaceX", 1);
                animator.SetFloat("FaceY", 0);
            }
            else
            {
                animator.SetFloat("FaceX", -1);
                animator.SetFloat("FaceY", 0);
            }
        }
        else
        {
            if (diff.y > 0)
            {
                animator.SetFloat("FaceX", 0);
                animator.SetFloat("FaceY", 1);
            }
            else
            {
                animator.SetFloat("FaceX", 0);
                animator.SetFloat("FaceY", -1);
            }
        }
    }
}
