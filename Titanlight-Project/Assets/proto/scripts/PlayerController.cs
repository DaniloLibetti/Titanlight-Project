using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // ============================
    // Configurações Gerais e Movimento
    // ============================
    [Header("Vida")]
    [SerializeField] private Health health;

    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    private Vector2 currentSmoothVelocity;
    public bool isStunned = false;

    [Header("Ajustes de Velocidade")]
    [SerializeField] private float chargingSpeedMultiplier = 0.5f;
    [SerializeField] private float machineGunSpeedMultiplier = 0.7f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float dashDamage = 15f;
    [SerializeField] private float dashAttackRadius = 0.7f;
    private float dashTimer = 0f;
    private bool isDashing = false;
    private bool canDash = true;

    [Header("Obstáculos")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    // ============================
    // Ataque Corpo a Corpo (Melee)
    // ============================
    [Header("Ataque Melee")]
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    // ============================
    // Ataques a Distância
    // ============================
    [Header("Ataques a Distância - Geral")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootPointDistance = 0.5f;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private float projectileLifetime = 0.5f;

    [Header("Arma Comum")]
    [SerializeField] private GameObject normalBulletPrefab;
    [SerializeField] private KeyCode attackKey = KeyCode.O;
    [SerializeField] private float normalCooldown = 0.5f;

    [Header("Shotgun")]
    [SerializeField] private GameObject shotgunBulletPrefab;
    [SerializeField] private KeyCode shotgunKey = KeyCode.K;
    [SerializeField] private float shotgunCooldown = 0.7f;
    [SerializeField] private int shotgunPelletCount = 6;
    [SerializeField] private float shotgunSpreadAngle = 45f;
    [SerializeField] private float shotgunRangeMultiplier = 1.0f;
    [SerializeField] private int extraPelletsMultiplier = 2;
    [SerializeField] private float extraSpreadAngleMultiplier = 10f;
    [SerializeField] private float baseRecoilForce = 2f;
    [SerializeField] private float minShotgunSpreadAngle = 10f;
    [SerializeField] private float maxShotgunLifetimeMultiplier = 2f;

    [Header("Metralhadora")]
    [SerializeField] private GameObject machineGunBulletPrefab;
    [SerializeField] private KeyCode machineGunKey = KeyCode.M;
    [SerializeField] private float machineGunCooldown = 0.2f;

    // ============================
    // Mecânica de Carregamento
    // ============================
    [Header("Mecânica de Carregamento")]
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float chargeDamageMultiplierMin = 1f;
    [SerializeField] private float chargeDamageMultiplierMax = 3f;
    private float currentChargeTime = 0f;
    private bool isCharging = false;

    // ============================
    // Sistema de Aquecimento
    // ============================
    [Header("Sistema de Aquecimento")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float heatIncreaseRate = 20f;
    [SerializeField] private float heatDecreaseRate = 15f;
    [SerializeField] private float overheatCooldownThreshold = 20f;
    private float currentHeat = 0f;
    private bool overheated = false;
    public float HeatProgress => currentHeat / maxHeat;

    // ============================
    // Controles Gerais
    // ============================
    [Header("Controles Gerais")]
    [SerializeField] private KeyCode dashKey = KeyCode.Space;
    [SerializeField] private KeyCode switchModeKey = KeyCode.Q;

    private bool canAttack = true;
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.right;

    private enum AttackMode { Melee, Ranged }
    [SerializeField] private AttackMode attackMode = AttackMode.Melee;

    private enum RangedAttackType { Normal, Shotgun, MachineGun }
    private RangedAttackType currentRangedAttackType = RangedAttackType.Normal;

    public float DashProgress => canDash ? 1f : 1f - (dashTimer / dashCooldown);
    public float ChargeProgress => isCharging ? (currentChargeTime / maxChargeTime) : 0f;
    public bool IsDashing => isDashing;
    public bool CanMove { get; set; } = true;
    public Vector3 LastDashPosition { get; private set; } = Vector3.zero;

    public EquipmentMenuController EquipController;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (health == null) health = GetComponent<Health>();
    }

    void Update()
    {
        if (isStunned || !CanMove)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimations();
            UpdateShootPointDirection();
            UpdateHeat();
            return;
        }

        if (!canDash)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                dashTimer = 0f;
                canDash = true;
            }
        }

        if (isDashing)
        {
            UpdateAnimations();
            UpdateShootPointDirection();
            UpdateHeat();
            return;
        }

        GetMovementInput();

        if (Input.GetKeyDown(switchModeKey))
            ToggleAttackMode();

        HandleAttacks();

        if (Input.GetKeyDown(dashKey) && canDash && lastDirection != Vector2.zero)
            StartCoroutine(Dash());

        UpdateAnimations();
        UpdateShootPointDirection();
        UpdateHeat();
    }

    void FixedUpdate()
    {
        if (!isDashing && !isStunned && CanMove)
            MoveCharacter();
    }

    void ToggleAttackMode()
    {
        attackMode = attackMode == AttackMode.Melee ? AttackMode.Ranged : AttackMode.Melee;
        spriteRenderer.color = (attackMode == AttackMode.Ranged) ? Color.green : Color.white;
    }

    void GetMovementInput()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (moveInput != Vector2.zero)
            lastDirection = moveInput;
    }

    void HandleAttacks()
    {
        if (!canAttack) return;

        if (attackMode == AttackMode.Melee && Input.GetKeyDown(attackKey))
            StartCoroutine(MeleeAttack());
        else if (attackMode == AttackMode.Ranged)
        {
            if (Input.GetKeyDown(attackKey) || Input.GetKeyDown(shotgunKey))
            {
                isCharging = true;
                currentChargeTime = 0f;
                currentRangedAttackType = Input.GetKeyDown(shotgunKey) ? RangedAttackType.Shotgun : RangedAttackType.Normal;
            }

            if (isCharging && (Input.GetKey(attackKey) || Input.GetKey(shotgunKey)))
            {
                currentChargeTime = Mathf.Min(currentChargeTime + Time.deltaTime, maxChargeTime);
            }

            if (isCharging && (Input.GetKeyUp(attackKey) || Input.GetKeyUp(shotgunKey)))
            {
                float mult = Mathf.Lerp(chargeDamageMultiplierMin, chargeDamageMultiplierMax, currentChargeTime / maxChargeTime);
                StartCoroutine(RangedAttackCharged(mult));
                isCharging = false;
            }

            if (Input.GetKey(machineGunKey))
            {
                currentRangedAttackType = RangedAttackType.MachineGun;
                if (!overheated)
                    StartCoroutine(RangedAttack());
            }
        }
    }

    void UpdateShootPointDirection()
    {
        if (shootPoint != null)
            shootPoint.localPosition = (Vector3)lastDirection * shootPointDistance;
    }

    void MoveCharacter()
    {
        float speedMod = 1f;
        if (isCharging) speedMod *= chargingSpeedMultiplier;
        if (currentRangedAttackType == RangedAttackType.MachineGun && Input.GetKey(machineGunKey))
            speedMod *= machineGunSpeedMultiplier;

        Vector2 targetVel = moveInput * moveSpeed * speedMod;
        float smoothTime = moveInput.magnitude > 0 ? 1f / acceleration : 1f / deceleration;
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVel, ref currentSmoothVelocity, smoothTime);
    }

    IEnumerator Dash()
    {
        // Ativa o bool IsDashing (no Animator, sem trigger)
        animator.SetBool("IsDashing", true);
        isDashing = true;
        canDash = false;
        dashTimer = dashCooldown;

        int originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Dashing");

        float startTime = Time.time;
        Vector2 dashDirection = lastDirection.normalized;
        var hitEnemies = new HashSet<Collider2D>();

        while (Time.time < startTime + dashDuration)
        {
            Vector2 displacement = dashDirection * dashSpeed * Time.fixedDeltaTime;
            if (Physics2D.CircleCast(rb.position, dashAttackRadius, dashDirection, displacement.magnitude, obstacleLayer))
                break;

            rb.MovePosition(rb.position + displacement);

            var enemies = Physics2D.OverlapCircleAll(rb.position, dashAttackRadius, enemyLayer);
            foreach (var e in enemies)
                if (!e.isTrigger && hitEnemies.Add(e))
                    e.GetComponent<Health>()?.TakeDamage(dashDamage);

            yield return new WaitForFixedUpdate();
        }

        gameObject.layer = originalLayer;
        LastDashPosition = transform.position;

        // Desativa o dash
        isDashing = false;
        animator.SetBool("IsDashing", false);
    }

    IEnumerator RangedAttack()
    {
        canAttack = false;
        animator.SetTrigger("RangedAttack");
        ShootProjectile(machineGunBulletPrefab, lastDirection);
        yield return new WaitForSeconds(machineGunCooldown);
        canAttack = true;
    }

    IEnumerator RangedAttackCharged(float multiplier)
    {
        canAttack = false;
        animator.SetTrigger("RangedAttack");

        if (currentRangedAttackType == RangedAttackType.Normal)
        {
            ShootProjectile(normalBulletPrefab, lastDirection, multiplier);
            yield return new WaitForSeconds(normalCooldown);
        }
        else // Shotgun
        {
            ShootChargedShotgun(multiplier);
            yield return new WaitForSeconds(shotgunCooldown);
        }

        canAttack = true;
    }

    void ShootProjectile(GameObject prefab, Vector2 dir, float mult = 1f)
    {
        if (prefab == null || shootPoint == null) return;
        var proj = Instantiate(prefab, shootPoint.position, Quaternion.identity);
        if (proj.TryGetComponent<Projectile>(out var script))
        {
            script.Initialize(dir, projectileSpeed, collisionLayers, this);
            script.damageMultiplier = mult;
        }
        Destroy(proj, projectileLifetime);
    }

    void ShootChargedShotgun(float mult)
    {
        int extra = Mathf.RoundToInt((mult - 1f) * extraPelletsMultiplier);
        int total = shotgunPelletCount + extra;
        float t = Mathf.InverseLerp(chargeDamageMultiplierMin, chargeDamageMultiplierMax, mult);
        float spread = Mathf.Lerp(shotgunSpreadAngle, minShotgunSpreadAngle, t);
        float lifeMult = Mathf.Lerp(1f, maxShotgunLifetimeMultiplier, t);
        float life = projectileLifetime * lifeMult;

        float baseAng = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
        for (int i = 0; i < total; i++)
        {
            float off = Random.Range(-spread, spread);
            float ang = baseAng + off;
            Vector2 dir = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad)).normalized;
            var origin = (Vector2)shootPoint.position + lastDirection * Random.Range(-0.1f, 0.1f);
            var p = Instantiate(shotgunBulletPrefab, origin, Quaternion.identity);
            if (p.TryGetComponent<Projectile>(out var s))
            {
                s.Initialize(dir, projectileSpeed * Random.Range(0.9f, 1.1f), collisionLayers, this);
                s.damageMultiplier = mult;
            }
            Destroy(p, life);
        }
        rb.AddForce(-lastDirection * baseRecoilForce * (mult - 1f), ForceMode2D.Impulse);
    }

    IEnumerator MeleeAttack()
    {
        canAttack = false;
        animator.SetTrigger("Attack");

        Vector2 pos = rb.position + lastDirection * attackDistance;
        var hits = Physics2D.OverlapCircleAll(pos, attackRadius, enemyLayer);
        foreach (var e in hits)
            if (!e.isTrigger)
                e.GetComponent<Health>()?.TakeDamage(attackDamage);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void UpdateAnimations()
    {
        Vector2 dir = moveInput != Vector2.zero ? moveInput : lastDirection;
        animator.SetBool("IsWalking", moveInput != Vector2.zero);
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        // o bool IsDashing já é atualizado diretamente no Dash()
    }

    private void UpdateHeat()
    {
        if (currentRangedAttackType == RangedAttackType.MachineGun && Input.GetKey(machineGunKey) && !overheated)
        {
            currentHeat = Mathf.Min(currentHeat + heatIncreaseRate * Time.deltaTime, maxHeat);
            if (currentHeat >= maxHeat)
                overheated = true;
        }
        else
        {
            currentHeat = Mathf.Max(currentHeat - heatDecreaseRate * Time.deltaTime, 0f);
            if (overheated && currentHeat <= overheatCooldownThreshold)
                overheated = false;
        }
    }

    public void TakeDamage(float amount)
    {
        health?.TakeDamage(amount);
    }

    public void Stun(float duration)
    {
        if (!isStunned) StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    void OnDrawGizmos()
    {
        if (shootPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(shootPoint.position, 0.1f);
            Gizmos.DrawLine(transform.position, shootPoint.position);
        }
    }
}
