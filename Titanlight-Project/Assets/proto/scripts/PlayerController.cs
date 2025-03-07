using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float dashDamage = 15f;
    [SerializeField] private float dashAttackRadius = 0.7f;

    [Header("Ataque Melee")]
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Ataque à Distância")]
    [SerializeField] private GameObject normalProjectilePrefab;
    [SerializeField] private GameObject chargedProjectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float chargeTime = 1f;
    [SerializeField] private float shootCooldown = 0.5f;

    private float chargeTimer = 0f;
    private bool isCharging = false;
    private bool canShoot = true;

    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Vector2 respawnPoint;
    private float currentHealth;

    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    private bool isDashing = false;
    private bool canDash = true;
    private bool canAttack = true;
    private bool isAttacking = false;
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.right;
    private AreaLimiter areaLimiter;

    private enum AttackMode
    {
        Melee,
        Ranged
    }

    [SerializeField] private AttackMode attackMode = AttackMode.Melee;

    void Start()
    {
        currentHealth = maxHealth;
        respawnPoint = transform.position;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        areaLimiter = FindObjectOfType<AreaLimiter>();
    }

    void Update()
    {
        if (isDashing || isAttacking) return;

        GetMovementInput();

        if (Input.GetKeyDown(KeyCode.T))
        {
            attackMode = attackMode == AttackMode.Melee ? AttackMode.Ranged : AttackMode.Melee;
        }

        if (attackMode == AttackMode.Melee)
        {
            HandleDash();
            HandleMeleeAttack();
        }
        else
        {
            HandleRangedAttack();
        }

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (!isDashing && !isAttacking)
        {
            MoveCharacter();
        }
    }

    void GetMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical).normalized;

        if (moveInput != Vector2.zero)
        {
            lastDirection = moveInput;
        }
    }

    void LimitPlayerMovement()
    {
        if (areaLimiter == null) return;
        rb.position = areaLimiter.ClampPosition(rb.position);
    }

    void MoveCharacter()
    {
        Vector2 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(areaLimiter.ClampPosition(newPosition));
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canDash && lastDirection != Vector2.zero)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        gameObject.layer = LayerMask.NameToLayer("Dashing");

        float startTime = Time.time;
        Vector2 dashDirection = lastDirection.normalized;
        HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

        while (Time.time < startTime + dashDuration)
        {
            Vector2 newPos = rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime;
            rb.MovePosition(areaLimiter.ClampPosition(newPos));

            Collider2D[] enemies = Physics2D.OverlapCircleAll(rb.position, dashAttackRadius, enemyLayer);
            foreach (Collider2D enemy in enemies)
            {
                if (!hitEnemies.Contains(enemy))
                {
                    hitEnemies.Add(enemy);
                    if (enemy.TryGetComponent<Health>(out Health health))
                    {
                        health.TakeDamage(dashDamage);
                    }
                }
            }

            yield return new WaitForFixedUpdate();
        }

        gameObject.layer = LayerMask.NameToLayer("Default");
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void HandleMeleeAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && canAttack)
        {
            StartCoroutine(MeleeAttack());
        }
    }

    IEnumerator MeleeAttack()
    {
        isAttacking = true;
        canAttack = false;
        animator.SetTrigger("Attack");
        rb.linearVelocity = Vector2.zero;

        Vector2 attackPosition = rb.position + lastDirection * attackDistance;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition, attackRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        canAttack = true;
    }

    void HandleRangedAttack()
    {
        if (attackMode != AttackMode.Ranged || !canShoot) return;

        if (Input.GetKeyDown(KeyCode.Mouse1)) // Botão direito do mouse
        {
            isCharging = true;
            chargeTimer = 0f;
        }

        if (isCharging)
        {
            chargeTimer += Time.deltaTime;

            if (Input.GetKeyUp(KeyCode.Mouse1)) // Botão direito do mouse
            {
                isCharging = false;
                if (chargeTimer >= chargeTime)
                {
                    ShootProjectile(chargedProjectilePrefab);
                }
                else
                {
                    ShootProjectile(normalProjectilePrefab);
                }
            }
        }
    }

    void ShootProjectile(GameObject projectilePrefab)
    {
        GameObject projectile = Instantiate(projectilePrefab, (Vector3)rb.position + (Vector3)lastDirection * 0.5f, Quaternion.identity);
        projectile.GetComponent<Projectile>().SetDirection(lastDirection);
        projectile.GetComponent<Projectile>().SetSpeed(projectileSpeed);

        StartCoroutine(RangedCooldown());
    }

    IEnumerator RangedCooldown()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    void UpdateAnimations()
    {
        if (moveInput != Vector2.zero)
        {
            animator.SetBool("IsWalking", true);
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        transform.position = respawnPoint;
        currentHealth = maxHealth;
    }

    public void ProjectileDestroyed()
    {
        canShoot = true; // Permite disparar novamente após destruição do projétil
    }
}
