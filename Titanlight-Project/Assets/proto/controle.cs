using System.Collections;
using UnityEngine;

public class Controle : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    [Header("Ataque Melee")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Referências")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;
    

    private bool isDashing = false;
    private bool canDash = true;
    private bool canAttack = true;
    private bool isAttacking = false;
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.right;
    private areaLimiter areaLimiter;


    void Start()
    {
        currentHealth = maxHealth;
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        areaLimiter = FindObjectOfType<areaLimiter>();
    }

    void Update()
    {
        if (isDashing || isAttacking) return;
        GetMovementInput();
        MoveCharacter();
        HandleDash();
        HandleMeleeAttack();
        UpdateAnimations();
    }

    void GetMovementInput()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveHorizontal, moveVertical).normalized;
        if (moveInput != Vector2.zero)
        {
            lastDirection = moveInput;
        }
    }

    void MoveCharacter()
    {
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.deltaTime;
        controller.Move(move);
        LimitPlayerMovement();
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canDash && lastDirection != Vector2.zero)
        {
            StartCoroutine(Dash());
        }
    }

    void HandleMeleeAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && canAttack)
        {
            StartCoroutine(MeleeAttack());
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        Vector3 dashDirection = (Vector3)lastDirection;
        float dashTime = 0f;

        while (dashTime < dashDuration)
        {
            Vector3 newPosition = transform.position + dashDirection * dashSpeed * Time.deltaTime;
            newPosition.x = Mathf.Clamp(newPosition.x, areaLimiter.MinBounds.x, areaLimiter.MaxBounds.x);
            newPosition.y = Mathf.Clamp(newPosition.y, areaLimiter.MinBounds.y, areaLimiter.MaxBounds.y);
            transform.position = newPosition;
            dashTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    IEnumerator MeleeAttack()
    {
        isAttacking = true;
        canAttack = false;
        animator.SetFloat("Horizontal", lastDirection.x);
        animator.SetFloat("Vertical", lastDirection.y);
        animator.SetTrigger("Attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            transform.position + (Vector3)lastDirection * attackRange,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(GetAttackAnimationDuration());
        isAttacking = false;
        canAttack = true;
    }

    float GetAttackAnimationDuration()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsTag("Attack"))
        {
            return stateInfo.length;
        }
        return 0.5f;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        animator.SetFloat("Horizontal", lastDirection.x);
        animator.SetFloat("Vertical", lastDirection.y);
        animator.SetBool("IsMoving", moveInput != Vector2.zero && !isAttacking);
        animator.SetBool("IsDashing", isDashing);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("Jogador Morreu!");
        gameObject.SetActive(false);
    }

    void LimitPlayerMovement()
    {
        if (areaLimiter == null) return;
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, areaLimiter.MinBounds.x, areaLimiter.MaxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, areaLimiter.MinBounds.y, areaLimiter.MaxBounds.y);
        transform.position = clampedPosition;
    }
}
