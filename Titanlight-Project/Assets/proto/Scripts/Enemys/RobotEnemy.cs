using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Health))]
public class RobotEnemy : MonoBehaviour
{
    [Header("Configurações de Combate")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float cooldownTime = 2f;
    [SerializeField] private float activationDistance = 5f;
    [SerializeField] private float attackDamage = 4f;

    [Header("Drop de Moedas")]
    [SerializeField] private EnemyCoinDrop coinDrop; // referência ao seu componente de drop

    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D attackCollider;

    private Health health;
    private Transform player;
    private bool isInCooldown = false;
    private bool isDead = false;
    private Vector2 dashDirection;
    private Collider2D[] allColliders;
    private SegmentedHealthBar healthBar;

    void Awake()
    {
        // obtém o Health e subscreve
        health = GetComponent<Health>();
        health.onDamageTaken.AddListener(OnDamageTaken);
        health.onDeath.AddListener(OnDeath);
        
        healthBar = GetComponent<SegmentedHealthBar>();

        // cache dos colliders para intangibilidade
        allColliders = GetComponents<Collider2D>();
    }

    void Start()
    {
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        attackCollider.isTrigger = true;
        attackCollider.enabled = false;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null || isInCooldown || isDead) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool viu = dist <= activationDistance;

        animator.SetBool("InimigoÀVista", viu);

        if (viu)
            StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isInCooldown = true;
        animator.SetTrigger("Ataque");
        yield return new WaitForSeconds(cooldownTime);
        isInCooldown = false;
    }

    // Animation Event no frame do dash
    public void PerformDash()
    {
        if (isDead || player == null) return;

        dashDirection = (player.position - transform.position).normalized;
        attackCollider.enabled = true;
        rb.linearVelocity = dashDirection * dashSpeed;

        StartCoroutine(EndDash());
    }

    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(dashDuration);
        rb.linearVelocity = Vector2.zero;
        attackCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!attackCollider.enabled || isDead) return;
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                //healthBar.UpdateHealthBar(attackDamage);
            }
                
        }
    }

    // Invocado pelo Health.onDamageTaken
    public void OnDamageTaken(float damageAmount)
    {
        if (isDead) return;
        animator.SetTrigger("Damage");
    }

    // Invocado pelo Health.onDeath
    public void OnDeath()
    {
        if (isDead) return;
        isDead = true;

        // torna intangível
        foreach (var col in allColliders)
            col.enabled = false;

        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        attackCollider.enabled = false;

        animator.SetTrigger("Death");
        // não destrói aqui, aguarda o Animation Event
    }

    // Animation Event no último frame de "Death"
    public void OnDeathAnimationEnd()
    {
        // faz o drop usando o método correto
        if (coinDrop != null)
            coinDrop.DropItems();

        Destroy(gameObject);
    }

    void FixedUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}
