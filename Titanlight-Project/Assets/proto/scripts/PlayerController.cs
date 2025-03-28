using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movimentacao")]
    [SerializeField] private float moveSpeed = 5f; // velocidade do movimento

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f; // velocidade do dash
    [SerializeField] private float dashDuration = 0.2f; // tempo do dash
    [SerializeField] private float dashCooldown = 0.5f; // tempo pra recarregar dash
    [SerializeField] private float dashDamage = 15f; // dano do dash
    [SerializeField] private float dashAttackRadius = 0.7f; // area do dash

    [Header("Obstaculos")]
    [SerializeField] private LayerMask obstacleLayer; // layer de obstaculos

    [Header("Ataque Melee")]
    [SerializeField] private float attackDistance = 1f; // distancia do ataque melee
    [SerializeField] private float attackRadius = 0.5f; // area do ataque melee
    [SerializeField] private float attackDamage = 10f; // dano do ataque melee
    [SerializeField] private float attackCooldown = 0.5f; // tempo pra recarregar ataque melee
    [SerializeField] private LayerMask enemyLayer; // layer dos inimigos

    [Header("Ataque a Distancia")]
    [SerializeField] private float projectileSpeed = 25f; // velocidade do projeteis
    [SerializeField] private float projectileLifetime = 0.5f; // tempo de vida do projeteis
    [SerializeField] private Transform shootPoint; // ponto de onde sai o tiro
    [SerializeField] private float shootPointDistance = 0.5f; // distancia do ponto de tiro
    [SerializeField] private LayerMask collisionLayers; // layers que o tiro colide

    [Header("Prefabs de Bullet")]
    [SerializeField] private GameObject normalBulletPrefab; // prefab do tiro normal
    [SerializeField] private GameObject shotgunBulletPrefab; // prefab do tiro shotgun
    [SerializeField] private GameObject machineGunBulletPrefab; // prefab do tiro metralhadora

    [Header("Controles de Armas a Distancia")]
    [SerializeField] private KeyCode attackKey = KeyCode.O;      // tiro normal (carregavel)
    [SerializeField] private KeyCode shotgunKey = KeyCode.K;       // tiro shotgun (carregavel)
    [SerializeField] private KeyCode machineGunKey = KeyCode.M;    // tiro metralhadora (disparo imediato)

    [Header("Configs - Ataque")]
    [SerializeField] private float normalCooldown = 0.5f; // tempo pra recarregar tiro normal
    [SerializeField] private float shotgunCooldown = 0.7f; // tempo pra recarregar tiro shotgun
    [SerializeField] private float machineGunCooldown = 0.2f; // tempo pra recarregar metralhadora

    [Header("Configs - Shotgun")]
    [SerializeField] private int shotgunPelletCount = 6; // qtd de projeteis da shotgun
    [SerializeField] private float shotgunSpreadAngle = 45f; // angulo de dispersao da shotgun
    [SerializeField] private float shotgunRangeMultiplier = 1.0f; // multiplica a area do tiro

    [Header("Mecanica de Carregamento")]
    [SerializeField] private float maxChargeTime = 2f; // tempo maximo de carregamento
    [SerializeField] private float chargeDamageMultiplierMin = 1f; // multiplicador minimo de dano carregado
    [SerializeField] private float chargeDamageMultiplierMax = 3f; // multiplicador maximo de dano carregado
    private float currentChargeTime = 0f;
    private bool isCharging = false;

    [Header("Controles Gerais")]
    [SerializeField] private KeyCode dashKey = KeyCode.Space; // tecla do dash
    [SerializeField] private KeyCode switchModeKey = KeyCode.Q; // tecla pra trocar modo de ataque

    [Header("Componentes")]
    [SerializeField] private Rigidbody2D rb; // rigidbody do player
    [SerializeField] private Animator animator; // animador do player
    [SerializeField] private SpriteRenderer spriteRenderer; // sprite do player

    private bool isDashing = false;
    private bool canDash = true;
    private bool canAttack = true;
    private bool isAttacking = false;
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.right; // direcao inicial pra direita

    private enum AttackMode { Melee, Ranged }
    [SerializeField] private AttackMode attackMode = AttackMode.Melee; // modo inicial: melee

    private enum RangedAttackType { Normal, Shotgun, MachineGun }
    private RangedAttackType currentRangedAttackType = RangedAttackType.Normal;

    void Start()
    {
        // pega os componentes se nao estiverem setados
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDashing)
        {
            // durante o dash, atualiza as animacoes e a direcao do tiro
            UpdateAnimations();
            UpdateShootPointDirection();
            return;
        }

        // pega input de movimento
        GetMovementInput();

        if (Input.GetKeyDown(switchModeKey))
        {
            ToggleAttackMode();
        }

        if (attackMode == AttackMode.Melee)
        {
            if (Input.GetKeyDown(attackKey) && canAttack)
            {
                StartCoroutine(MeleeAttack());
            }
        }
        else if (attackMode == AttackMode.Ranged && canAttack)
        {
            // inicia o carregamento para tiros normal e shotgun
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
            // tiro rapido pra metralhadora
            if (Input.GetKeyDown(machineGunKey))
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
        UpdateShootPointDirection();
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            // move o personagem
            MoveCharacter();
        }
    }

    void ToggleAttackMode()
    {
        // troca entre melee e ranged
        attackMode = attackMode == AttackMode.Melee ? AttackMode.Ranged : AttackMode.Melee;
        if (spriteRenderer != null)
        {
            // muda a cor pra indicar o modo: verde pra ranged, branco pra melee
            spriteRenderer.color = (attackMode == AttackMode.Ranged) ? Color.green : Color.white;
        }
        Debug.Log($"Modo de Ataque: {attackMode}");
    }

    void GetMovementInput()
    {
        // pega o input de movimento do teclado
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical).normalized;
        if (moveInput != Vector2.zero)
        {
            lastDirection = moveInput;
        }
    }

    void UpdateShootPointDirection()
    {
        // atualiza a posicao do ponto de tiro baseado na ultima direcao
        if (shootPoint != null)
        {
            shootPoint.localPosition = lastDirection * shootPointDistance;
        }
    }

    void MoveCharacter()
    {
        // move o personagem
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
            // calcula o deslocamento do dash
            Vector2 displacement = dashDirection * dashSpeed * Time.fixedDeltaTime;
            Vector2 newPos = rb.position + displacement;

            // verifica colisao com obstaculos
            RaycastHit2D hit = Physics2D.CircleCast(rb.position, dashAttackRadius, dashDirection, displacement.magnitude, obstacleLayer);
            if (hit.collider != null)
            {
                break;
            }

            rb.MovePosition(newPos);

            // verifica se acertou inimigos
            Collider2D[] enemies = Physics2D.OverlapCircleAll(rb.position, dashAttackRadius, enemyLayer);
            foreach (Collider2D enemy in enemies)
            {
                if (enemy.isTrigger) continue; // ignora triggers

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
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

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

    void ShootProjectile(GameObject bulletPrefab, Vector2 direction, float damageMultiplier = 1f)
    {
        // cria o projetei e o inicializa
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

    void ShootChargedShotgun(float damageMultiplier)
    {
        // atira varias balas com um spread
        float baseAngle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
        for (int i = 0; i < shotgunPelletCount; i++)
        {
            float offset = Random.Range(-shotgunSpreadAngle / 2f, shotgunSpreadAngle / 2f);
            float finalAngle = baseAngle + offset;
            Vector2 pelletDirection = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad)) * shotgunRangeMultiplier;
            ShootProjectile(shotgunBulletPrefab, pelletDirection, damageMultiplier);
        }
    }

    IEnumerator MeleeAttack()
    {
        isAttacking = true;
        canAttack = false;
        animator.SetTrigger("Attack");

        // calcula a posicao de ataque e checa colisao com inimigos
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

    void UpdateAnimations()
    {
        // atualiza as animacoes de movimento
        animator.SetBool("IsWalking", moveInput != Vector2.zero);
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);
    }

    // metodos dummy pra referencias externas
    public void ProjectileDestroyed() { }
    public void TakeDamage(float amount) { }

    void OnDrawGizmos()
    {
        // desenha o ponto de tiro no editor
        if (shootPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(shootPoint.position, 0.1f);
            Gizmos.DrawLine(transform.position, shootPoint.position);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Vector3 defaultPoint = transform.position + (Vector3)(lastDirection * shootPointDistance);
            Gizmos.DrawWireSphere(defaultPoint, 0.1f);
            Gizmos.DrawLine(transform.position, defaultPoint);
        }
    }
}
