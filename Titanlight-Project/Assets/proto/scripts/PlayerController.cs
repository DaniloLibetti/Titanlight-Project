using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // ============================
    // Configurações Gerais e Movimento
    // ============================
    [Header("Vida")]
    [SerializeField] private Health health; // Componente de vida do player

    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 5f; // Velocidade máxima do movimento
    [SerializeField] private float acceleration = 10f;   // Quanto menor o valor, mais rápido acelera
    [SerializeField] private float deceleration = 15f;   // Quanto menor o valor, mais rápido desacelera
    private Vector2 currentSmoothVelocity;               // Auxiliar para o SmoothDamp
    public bool isStunned = false;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f; // Velocidade do dash
    [SerializeField] private float dashDuration = 0.2f; // Tempo do dash
    [SerializeField] private float dashCooldown = 0.5f; // Tempo para recarregar o dash
    [SerializeField] private float dashDamage = 15f; // Dano causado pelo dash
    [SerializeField] private float dashAttackRadius = 0.7f; // Área do dash
    private float dashTimer = 0f; // Cronômetro para o cooldown do dash

    [Header("Obstáculos")]
    [SerializeField] private LayerMask obstacleLayer; // Layer dos obstáculos

    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb; // Rigidbody do player
    [SerializeField] private Animator animator; // Animator do player
    [SerializeField] private SpriteRenderer spriteRenderer; // SpriteRenderer do player

    // ============================
    // Ataque Corpo a Corpo (Melee)
    // ============================
    [Header("Ataque Melee")]
    [SerializeField] private float attackDistance = 1f; // Distância do ataque melee
    [SerializeField] private float attackRadius = 0.5f; // Área do ataque melee
    [SerializeField] private float attackDamage = 10f; // Dano do ataque melee
    [SerializeField] private float attackCooldown = 0.5f; // Tempo para recarregar o ataque melee
    [SerializeField] private LayerMask enemyLayer; // Layer dos inimigos

    // ============================
    // Ataques a Distância
    // ============================
    [Header("Ataques a Distância - Geral")]
    [SerializeField] private Transform shootPoint; // Ponto de onde sai o tiro
    [SerializeField] private float shootPointDistance = 0.5f; // Distância do ponto de tiro
    [SerializeField] private LayerMask collisionLayers; // Layers com que o tiro colide
    [SerializeField] private float projectileSpeed = 25f; // Velocidade dos projéteis
    [SerializeField] private float projectileLifetime = 0.5f; // Tempo de vida padrão dos projéteis

    // ----- Arma Comum -----
    [Header("Ataques a Distância - Arma Comum")]
    [SerializeField] private GameObject normalBulletPrefab; // Prefab do tiro normal
    [SerializeField] private KeyCode attackKey = KeyCode.O;      // Tecla para tiro normal (carregável)
    [SerializeField] private float normalCooldown = 0.5f; // Tempo para recarregar o tiro normal

    // ----- Shotgun -----
    [Header("Ataques a Distância - Shotgun")]
    [SerializeField] private GameObject shotgunBulletPrefab; // Prefab do tiro shotgun
    [SerializeField] private KeyCode shotgunKey = KeyCode.K;       // Tecla para tiro shotgun (carregável)
    [SerializeField] private float shotgunCooldown = 0.7f; // Tempo para recarregar o tiro shotgun
    [SerializeField] private int shotgunPelletCount = 6; // Quantidade base de projéteis da shotgun
    [SerializeField] private float shotgunSpreadAngle = 45f; // Ângulo base de dispersão da shotgun
    [SerializeField] private float shotgunRangeMultiplier = 1.0f; // Multiplicador da área do tiro
    [SerializeField] private int extraPelletsMultiplier = 2;       // Projéteis extras por unidade acima de 1 no multiplicador
    [SerializeField] private float extraSpreadAngleMultiplier = 10f; // Ângulo extra (graus) por unidade acima de 1
    [SerializeField] private float baseRecoilForce = 2f;             // Força base de recoil aplicada ao player

    // ----- Metralhadora -----
    [Header("Ataques a Distância - Metralhadora")]
    [SerializeField] private GameObject machineGunBulletPrefab; // Prefab do tiro metralhadora
    [SerializeField] private KeyCode machineGunKey = KeyCode.M;    // Tecla para tiro metralhadora (disparo contínuo)
    [SerializeField] private float machineGunCooldown = 0.2f; // Tempo para recarregar a metralhadora

    // ============================
    // Mecânica de Carregamento
    // ============================
    [Header("Mecânica de Carregamento")]
    [SerializeField] private float maxChargeTime = 2f; // Tempo máximo de carregamento
    [SerializeField] private float chargeDamageMultiplierMin = 1f; // Multiplicador mínimo de dano carregado
    [SerializeField] private float chargeDamageMultiplierMax = 3f; // Multiplicador máximo de dano carregado
    private float currentChargeTime = 0f;
    private bool isCharging = false;

    // ============================
    // Sistema de Aquecimento
    // ============================
    [Header("Sistema de Aquecimento")]
    [SerializeField] private float maxHeat = 100f;             // Calor máximo da arma
    [SerializeField] private float heatIncreaseRate = 20f;       // Taxa de aumento do calor (por segundo) enquanto atira
    [SerializeField] private float heatDecreaseRate = 15f;       // Taxa de diminuição do calor (por segundo) quando não atira
    [SerializeField] private float overheatCooldownThreshold = 20f; // Valor mínimo de calor para liberar a arma após superaquecimento
    private float currentHeat = 0f;
    private bool overheated = false;

    // Propriedade pública para UI (calor normalizado entre 0 e 1)
    public float HeatProgress
    {
        get { return currentHeat / maxHeat; }
    }

    // ============================
    // Controles Gerais e Configuração de Modo de Ataque
    // ============================
    [Header("Controles Gerais")]
    [SerializeField] private KeyCode dashKey = KeyCode.Space; // Tecla do dash
    [SerializeField] private KeyCode switchModeKey = KeyCode.Q; // Tecla para trocar modo de ataque

    private bool isDashing = false;
    private bool canDash = true;
    private bool canAttack = true;
    private bool isAttacking = false;
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.right; // Direção inicial para a direita

    private enum AttackMode { Melee, Ranged }
    [SerializeField] private AttackMode attackMode = AttackMode.Melee; // Modo inicial: melee

    private enum RangedAttackType { Normal, Shotgun, MachineGun }
    private RangedAttackType currentRangedAttackType = RangedAttackType.Normal;

    // Propriedades públicas para a UI dos outros sistemas
    public float DashProgress
    {
        get
        {
            if (canDash)
                return 1f;
            return 1f - (dashTimer / dashCooldown);
        }
    }

    public float ChargeProgress
    {
        get
        {
            return isCharging ? (currentChargeTime / maxChargeTime) : 0f;
        }
    }

    // ============================
    // Métodos do Ciclo de Vida e Movimento
    // ============================
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (health == null)
            health = GetComponent<Health>();
    }

    void Update()
    {
        // Se o player estiver stunado, interrompe a captura de input e zera a velocidade
        if (isStunned)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimations();
            UpdateShootPointDirection();
            UpdateHeat();
            return;
        }

        // Atualiza o cooldown do dash
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
        {
            ToggleAttackMode();
        }

        // Modo de ataque corpo a corpo
        if (attackMode == AttackMode.Melee)
        {
            if (Input.GetKeyDown(attackKey) && canAttack)
            {
                StartCoroutine(MeleeAttack());
            }
        }
        // Modo de ataque a distância
        else if (attackMode == AttackMode.Ranged && canAttack)
        {
            // Tiro carregado para arma comum e shotgun
            if (Input.GetKeyDown(attackKey))
            {
                isCharging = true;
                currentChargeTime = 0f;
                currentRangedAttackType = RangedAttackType.Normal;
            }
            if (Input.GetKeyDown(shotgunKey))
            {
                isCharging = true;
                currentChargeTime = 0f;
                currentRangedAttackType = RangedAttackType.Shotgun;
            }
            if (isCharging && (Input.GetKey(attackKey) || Input.GetKey(shotgunKey)))
            {
                currentChargeTime += Time.deltaTime;
                currentChargeTime = Mathf.Min(currentChargeTime, maxChargeTime);
            }
            if (isCharging && (Input.GetKeyUp(attackKey) || Input.GetKeyUp(shotgunKey)))
            {
                float multiplier = Mathf.Lerp(chargeDamageMultiplierMin, chargeDamageMultiplierMax, currentChargeTime / maxChargeTime);
                StartCoroutine(RangedAttackCharged(multiplier));
                isCharging = false;
                currentChargeTime = 0f;
            }
            // Tiro contínuo da metralhadora
            if (Input.GetKey(machineGunKey))
            {
                currentRangedAttackType = RangedAttackType.MachineGun;
                if (!overheated && canAttack)
                {
                    StartCoroutine(RangedAttack());
                }
            }
        }

        if (Input.GetKeyDown(dashKey) && canDash && lastDirection != Vector2.zero)
        {
            StartCoroutine(Dash());
        }

        UpdateAnimations();
        UpdateShootPointDirection();
        UpdateHeat();
    }

    void FixedUpdate()
    {
        if (!isDashing && !isStunned)
        {
            MoveCharacter();
        }
    }

    // Alterna entre modos de ataque
    void ToggleAttackMode()
    {
        attackMode = attackMode == AttackMode.Melee ? AttackMode.Ranged : AttackMode.Melee;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = (attackMode == AttackMode.Ranged) ? Color.green : Color.white;
        }
        Debug.Log($"Modo de Ataque: {attackMode}");
    }

    // Captura a entrada para movimento
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

    // Atualiza a posição do ponto de tiro conforme a direção do player
    void UpdateShootPointDirection()
    {
        if (shootPoint != null)
        {
            shootPoint.localPosition = lastDirection * shootPointDistance;
        }
    }

    // Movimento suave utilizando SmoothDamp
    void MoveCharacter()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;
        float smoothTime = (moveInput.magnitude > 0) ? 1f / acceleration : 1f / deceleration;
        Vector2 newVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentSmoothVelocity, smoothTime);
        rb.linearVelocity = newVelocity;
    }

    // ============================
    // Sistema de Dash e Ataques
    // ============================
    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        dashTimer = dashCooldown; // Reinicia o timer do dash
        int originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Dashing");

        float startTime = Time.time;
        Vector2 dashDirection = lastDirection.normalized;
        HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

        while (Time.time < startTime + dashDuration)
        {
            Vector2 displacement = dashDirection * dashSpeed * Time.fixedDeltaTime;
            Vector2 newPos = rb.position + displacement;

            RaycastHit2D hit = Physics2D.CircleCast(rb.position, dashAttackRadius, dashDirection, displacement.magnitude, obstacleLayer);
            if (hit.collider != null)
            {
                break;
            }

            rb.MovePosition(newPos);

            Collider2D[] enemies = Physics2D.OverlapCircleAll(rb.position, dashAttackRadius, enemyLayer);
            foreach (Collider2D enemy in enemies)
            {
                if (enemy.isTrigger) continue;
                if (!hitEnemies.Contains(enemy))
                {
                    hitEnemies.Add(enemy);
                    Health enemyHealth = enemy.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(dashDamage);
                    }
                }
            }
            yield return new WaitForFixedUpdate();
        }

        gameObject.layer = originalLayer;
        isDashing = false;
    }

    // Tiro para a metralhadora (arma de disparo contínuo)
    IEnumerator RangedAttack()
    {
        isAttacking = true;
        canAttack = false;
        animator.SetTrigger("RangedAttack");
        ShootProjectile(machineGunBulletPrefab, lastDirection);
        yield return new WaitForSeconds(machineGunCooldown);
        isAttacking = false;
        canAttack = true;
    }

    // Tiro carregado para armas comum e shotgun
    IEnumerator RangedAttackCharged(float damageMultiplier)
    {
        isAttacking = true;
        canAttack = false;
        animator.SetTrigger("RangedAttack");

        switch (currentRangedAttackType)
        {
            case RangedAttackType.Normal:
                ShootProjectile(normalBulletPrefab, lastDirection, damageMultiplier);
                yield return new WaitForSeconds(normalCooldown);
                break;
            case RangedAttackType.Shotgun:
                ShootChargedShotgun(damageMultiplier);
                yield return new WaitForSeconds(shotgunCooldown);
                break;
            default:
                break;
        }

        isAttacking = false;
        canAttack = true;
    }

    // Dispara um projétil para armas comuns e metralhadora
    void ShootProjectile(GameObject bulletPrefab, Vector2 direction, float damageMultiplier = 1f)
    {
        if (shootPoint != null && bulletPrefab != null)
        {
            GameObject projectile = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, projectileSpeed, collisionLayers, this);
                projectileScript.damageMultiplier = damageMultiplier;
                Destroy(projectile, projectileLifetime);
            }
        }
    }

    // Dispara os projéteis da shotgun com variações na direção, velocidade e posição
    void ShootChargedShotgun(float damageMultiplier)
    {
        int extraPellets = Mathf.RoundToInt((damageMultiplier - 1f) * extraPelletsMultiplier);
        int totalPellets = shotgunPelletCount + extraPellets;
        float extraSpread = (damageMultiplier - 1f) * extraSpreadAngleMultiplier;
        float totalSpreadAngle = shotgunSpreadAngle + extraSpread;
        float baseAngle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;

        for (int i = 0; i < totalPellets; i++)
        {
            float offset = Random.Range(-totalSpreadAngle, totalSpreadAngle); // Variação no ângulo para desorganização
            float finalAngle = baseAngle + offset;
            float speedFactor = Random.Range(0.9f, 1.1f); // Variação sutil na velocidade
            float pelletSpeed = projectileSpeed * speedFactor;
            Vector2 direction = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad)).normalized;
            Vector2 pelletOrigin = (Vector2)shootPoint.position + lastDirection * Random.Range(-0.1f, 0.1f); // Variação na posição

            GameObject projectile = Instantiate(shotgunBulletPrefab, pelletOrigin, Quaternion.identity);
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, pelletSpeed, collisionLayers, this);
                projectileScript.damageMultiplier = damageMultiplier;
            }
            Destroy(projectile, projectileLifetime);
        }

        float recoilForce = baseRecoilForce * (damageMultiplier - 1f);
        rb.AddForce(-lastDirection * recoilForce, ForceMode2D.Impulse);
    }

    // Ataque corpo a corpo (melee)
    IEnumerator MeleeAttack()
    {
        isAttacking = true;
        canAttack = false;
        animator.SetTrigger("Attack");

        Vector2 attackPosition = rb.position + lastDirection * attackDistance;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition, attackRadius, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.isTrigger) continue;
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        canAttack = true;
    }

    // Atualiza as animações do personagem conforme o movimento
    void UpdateAnimations()
    {
        // Se não houver movimento, usa a última direção conhecida
        Vector2 animDirection = (moveInput != Vector2.zero) ? moveInput : lastDirection;

        animator.SetBool("IsWalking", moveInput != Vector2.zero);
        animator.SetFloat("MoveX", animDirection.x);
        animator.SetFloat("MoveY", animDirection.y);
    }

    // Métodos dummy para referências externas
    public void ProjectileDestroyed() { }

    // Método de TakeDamage: Reduz a vida chamando o componente Health
    public void TakeDamage(float amount)
    {
        if (health != null)
        {
            health.TakeDamage(amount);
            Debug.Log("Player took damage: " + amount);
        }
        else
        {
            Debug.LogWarning("Health component not assigned!");
        }
    }

    // ============================
    // Sistema de Aquecimento: Atualiza o calor com base na ação de atirar ou não
    // ============================
    private void UpdateHeat()
    {
        // Se a metralhadora estiver sendo disparada e a arma não estiver superaquecida, aumenta o calor
        if (currentRangedAttackType == RangedAttackType.MachineGun && Input.GetKey(machineGunKey) && !overheated)
        {
            currentHeat += heatIncreaseRate * Time.deltaTime;
            if (currentHeat >= maxHeat)
            {
                currentHeat = maxHeat;
                overheated = true;
                Debug.Log("Arma superaquecida!");
            }
        }
        // Caso contrário, o calor diminui com o tempo
        else
        {
            currentHeat -= heatDecreaseRate * Time.deltaTime;
            if (currentHeat < 0)
            {
                currentHeat = 0;
            }
            // Se a arma estava superaquecida, libera o disparo quando o calor desce abaixo do limiar
            if (overheated && currentHeat <= overheatCooldownThreshold)
            {
                overheated = false;
                Debug.Log("Arma resfriada, pode atirar novamente!");
            }
        }
    }

    // ============================
    // Sistema de Stun: Impede a entrada de movimento e outras ações
    // ============================
    public void Stun(float duration)
    {
        if (!isStunned)
            StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        rb.linearVelocity = Vector2.zero;
        // Opcional: pode-se alterar a cor ou disparar uma animação de stun aqui
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    // ============================
    // Debug: Visualização dos pontos de tiro
    // ============================
    void OnDrawGizmos()
    {
        if (shootPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(shootPoint.position, 0.1f);
            Gizmos.DrawLine(transform.position, shootPoint.position);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Vector3 defaultPoint = transform.position + new Vector3(lastDirection.x, lastDirection.y, 0f) * shootPointDistance;
            Gizmos.DrawWireSphere(defaultPoint, 0.1f);
            Gizmos.DrawLine(transform.position, defaultPoint);
        }
    }
}
