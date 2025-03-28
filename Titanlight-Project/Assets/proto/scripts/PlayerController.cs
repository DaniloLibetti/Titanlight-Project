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
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private float projectileLifetime = 0.5f;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private LayerMask collisionLayers;

    // Chaves para alternar tipos de ataque à distância
    [Header("Controles de Armas à Distância")]
    [SerializeField] private KeyCode attackKey = KeyCode.O; // Para ataque normal ou melee
    [SerializeField] private KeyCode shotgunKey = KeyCode.K;  // Ataque shotgun
    [SerializeField] private KeyCode machineGunKey = KeyCode.M; // Ataque metralhadora

    // Cooldowns específicos para cada tipo de disparo (configuráveis no inspetor)
    [Header("Configurações - Ataque Normal")]
    [SerializeField] private float normalCooldown = 0.5f;

    [Header("Configurações - Ataque Shotgun")]
    [SerializeField] private float shotgunCooldown = 0.7f;

    [Header("Configurações - Ataque Metralhadora")]
    [SerializeField] private float machineGunCooldown = 0.2f;

    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Vector2 respawnPoint;
    private float currentHealth;

    [Header("Controles Gerais")]
    [SerializeField] private KeyCode dashKey = KeyCode.Space;
    [SerializeField] private KeyCode switchModeKey = KeyCode.Q;

    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer; // Referência ao SpriteRenderer

    private bool isDashing = false;
    private bool canDash = true;
    private bool canAttack = true;
    private bool isAttacking = false;
    private bool canShoot = true;
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.right;

    private enum AttackMode
    {
        Melee,
        Ranged
    }
    [SerializeField] private AttackMode attackMode = AttackMode.Melee;

    // Tipos de ataque à distância
    private enum RangedAttackType
    {
        Normal,
        Shotgun,
        MachineGun
    }
    private RangedAttackType currentRangedAttackType = RangedAttackType.Normal;

    void Start()
    {
        currentHealth = maxHealth;
        respawnPoint = transform.position;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDashing || isAttacking) return;

        GetMovementInput();

        if (Input.GetKeyDown(switchModeKey))
        {
            ToggleAttackMode();
        }

        // Modo Melee
        if (attackMode == AttackMode.Melee && Input.GetKeyDown(attackKey) && canAttack)
        {
            StartCoroutine(MeleeAttack());
        }
        // Modo Ranged
        else if (attackMode == AttackMode.Ranged && canAttack)
        {
            if (Input.GetKeyDown(attackKey))
            {
                currentRangedAttackType = RangedAttackType.Normal;
                StartCoroutine(RangedAttack());
            }
            else if (Input.GetKeyDown(shotgunKey))
            {
                currentRangedAttackType = RangedAttackType.Shotgun;
                StartCoroutine(RangedAttack());
            }
            else if (Input.GetKeyDown(machineGunKey))
            {
                currentRangedAttackType = RangedAttackType.MachineGun;
                StartCoroutine(RangedAttack());
            }
        }

        if (Input.GetKeyDown(dashKey) && canDash && lastDirection != Vector2.zero)
        {
            StartCoroutine(Dash());
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

    // Altera o modo de ataque e atualiza a cor do SpriteRenderer
    void ToggleAttackMode()
    {
        attackMode = attackMode == AttackMode.Melee ? AttackMode.Ranged : AttackMode.Melee;
        if (spriteRenderer != null)
        {
            if (attackMode == AttackMode.Ranged)
            {
                spriteRenderer.color = Color.green;
            }
            else
            {
                spriteRenderer.color = Color.white;
            }
        }
        Debug.Log($"Modo de Ataque: {attackMode}");
    }

    void GetMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical).normalized;

        if (moveInput != Vector2.zero)
        {
            lastDirection = moveInput;
            UpdateShootPointDirection();
        }
    }

    void UpdateShootPointDirection()
    {
        if (shootPoint != null)
        {
            shootPoint.localPosition = lastDirection * 0.5f;
        }
    }

    void MoveCharacter()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        int originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Dashing");

        float startTime = Time.time;
        Vector2 dashDirection = lastDirection.normalized;
        HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

        while (Time.time < startTime + dashDuration)
        {
            Vector2 newPos = rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);

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

        gameObject.layer = originalLayer;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    IEnumerator RangedAttack()
    {
        isAttacking = true;
        canShoot = false;
        animator.SetTrigger("RangedAttack");

        switch (currentRangedAttackType)
        {
            case RangedAttackType.Normal:
                if (shootPoint != null && projectilePrefab != null)
                {
                    GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
                    Projectile projectileScript = projectile.GetComponent<Projectile>();
                    if (projectileScript != null)
                    {
                        projectileScript.Initialize(lastDirection, projectileSpeed, collisionLayers, this);
                        Destroy(projectile, projectileLifetime);
                    }
                }
                yield return new WaitForSeconds(normalCooldown);
                break;

            case RangedAttackType.Shotgun:
                int pelletCount = 5;
                float spreadAngle = 30f;
                for (int i = 0; i < pelletCount; i++)
                {
                    float angleOffset = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
                    Quaternion rotationOffset = Quaternion.Euler(0, 0, angleOffset);
                    Vector2 pelletDirection = rotationOffset * lastDirection;
                    GameObject pellet = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
                    Projectile pelletScript = pellet.GetComponent<Projectile>();
                    if (pelletScript != null)
                    {
                        pelletScript.Initialize(pelletDirection, projectileSpeed, collisionLayers, this);
                        Destroy(pellet, projectileLifetime);
                    }
                }
                yield return new WaitForSeconds(shotgunCooldown);
                break;

            case RangedAttackType.MachineGun:
                if (shootPoint != null && projectilePrefab != null)
                {
                    GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
                    Projectile projectileScript = projectile.GetComponent<Projectile>();
                    if (projectileScript != null)
                    {
                        projectileScript.Initialize(lastDirection, projectileSpeed, collisionLayers, this);
                        Destroy(projectile, projectileLifetime);
                    }
                }
                yield return new WaitForSeconds(machineGunCooldown);
                break;
        }

        isAttacking = false;
        canShoot = true;
    }

    // Método chamado quando um projétil é destruído (caso o projétil chame essa função)
    public void ProjectileDestroyed()
    {
        canShoot = true;
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

    void UpdateAnimations()
    {
        animator.SetBool("IsWalking", moveInput != Vector2.zero);
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);
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
}
