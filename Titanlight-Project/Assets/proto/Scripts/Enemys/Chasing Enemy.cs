using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Health))]
public class ChasingEnemy : MonoBehaviour
{
    [Header("Configurações de Spawn")]
    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private float spawnAnimDuration = 0.7f;

    [Header("Configurações de Combate")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackDamage = 3f;
    [SerializeField] private float cooldownTime = 1.5f;
    [SerializeField] private float stunDuration = 0.5f;

    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private CircleCollider2D attackCollider;

    private Transform targetPlayer;
    private Transform[] players;
    private Health health;
    private LayerMask holeLayer;
    private Collider2D roomCollider;

    private bool isInCooldown = false;
    private bool isStunned = false;
    private bool isDead = false;
    private bool isActivated = false;

    void Awake()
    {
        health = GetComponent<Health>();
        health.onDamageTaken.AddListener(OnDamageTaken);
        health.onDeath.AddListener(OnDeath);

        GameObject p1 = GameObject.FindGameObjectWithTag("player 1");
        GameObject p2 = GameObject.FindGameObjectWithTag("player 2");
        players = new Transform[] { p1?.transform, p2?.transform };

        holeLayer = LayerMask.GetMask("HoleFloor");
    }

    void Start()
    {
        attackCollider.isTrigger = true;
        attackCollider.enabled = false;

        if (animator != null)
        {
            animator.SetFloat("MoveX", 0);
            animator.SetFloat("MoveY", -1);
            animator.SetBool("IsMoving", false);
        }
    }

    void Update()
    {
        if (!isActivated || isDead || isStunned || isInCooldown)
            return;

        targetPlayer = GetNearestPlayerInRoom();
        if (targetPlayer == null)
            return;

        float dist = Vector2.Distance(transform.position, targetPlayer.position);
        if (dist <= attackRange)
        {
            UpdateAnimationDirection(targetPlayer.position - transform.position);
            StartCoroutine(AttackRoutine());
        }
        else
        {
            MoveTowardTarget();
        }
    }

    public void ActivateInRoom(Collider2D roomArea)
    {
        roomCollider = roomArea;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (animator != null)
            animator.SetTrigger("Spawn");

        yield return new WaitForSeconds(spawnAnimDuration);
        isActivated = true;
    }

    private Transform GetNearestPlayerInRoom()
    {
        Transform nearest = null;
        float minDist = float.MaxValue;
        foreach (var p in players)
        {
            if (p == null || roomCollider == null)
                continue;

            if (!roomCollider.OverlapPoint(p.position))
                continue;

            float d = Vector2.Distance(transform.position, p.position);
            if (d < minDist)
            {
                nearest = p;
                minDist = d;
            }
        }
        return nearest;
    }

    private void MoveTowardTarget()
    {
        Vector2 dir = (targetPlayer.position - transform.position).normalized;
        UpdateAnimationDirection(dir);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir,
            Vector2.Distance(transform.position, targetPlayer.position), holeLayer);

        Vector2 moveDir;
        if (hit.collider != null)
        {
            Vector2 perp = Vector2.Perpendicular(dir).normalized;
            float side = UnityEngine.Random.value < 0.5f ? 1f : -1f;
            Vector2 detourPoint = hit.point + perp * side * 1.5f;
            moveDir = (detourPoint - (Vector2)transform.position).normalized;
            UpdateAnimationDirection(moveDir);
        }
        else
        {
            moveDir = dir;
        }

        rb.linearVelocity = moveDir * moveSpeed;

        if (animator != null)
            animator.SetBool("IsMoving", true);
    }

    private void UpdateAnimationDirection(Vector2 direction)
    {
        if (animator == null) return;

        direction.Normalize();
        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveY", direction.y);
    }

    private IEnumerator AttackRoutine()
    {
        isInCooldown = true;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetBool("IsMoving", false);

        if (animator != null)
            animator.SetTrigger("Attack");

        PositionAttackCollider();
        attackCollider.enabled = true;

        yield return new WaitForSeconds(0.2f);
        attackCollider.enabled = false;
        yield return new WaitForSeconds(cooldownTime);
        isInCooldown = false;
    }

    private void PositionAttackCollider()
    {
        Vector2 attackDir = new Vector2(
            animator.GetFloat("MoveX"),
            animator.GetFloat("MoveY")
        ).normalized;

        if (attackDir.magnitude < 0.1f) attackDir = Vector2.down;

        attackCollider.offset = attackDir * attackRange * 0.5f;
        attackCollider.radius = attackRange * 0.5f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!attackCollider.enabled || isDead) return;

        if (other.CompareTag("player 1") || other.CompareTag("player 2"))
        {
            other.GetComponent<Health>()?.TakeDamage(attackDamage);
        }
    }

    public void OnDamageTaken(float dmg)
    {
        if (isDead) return;
        StopAllCoroutines();
        isStunned = true;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Hurt");
        }

        StartCoroutine(StunRecovery());
    }

    private IEnumerator StunRecovery()
    {
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
    }

    public void OnDeath()
    {
        if (isDead) return;
        isDead = true;
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        attackCollider.enabled = false;

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Death");
        }

        StartCoroutine(DeactivateAfterAnimation());
    }

    private IEnumerator DeactivateAfterAnimation()
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        if (roomCollider != null)
        {
            Gizmos.color = Color.cyan;
            var bounds = roomCollider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}