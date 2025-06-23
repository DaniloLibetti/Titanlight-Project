using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Health))]
public class MimicChestTurret : MonoBehaviour
{
    [Header("Detecção")]
    [SerializeField] private float detectionRadius = 2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Disparo")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePointUp1;
    [SerializeField] private Transform firePointUp2;
    [SerializeField] private Transform firePointDown1;
    [SerializeField] private Transform firePointDown2;
    [SerializeField] private Transform firePointLeft1;
    [SerializeField] private Transform firePointLeft2;
    [SerializeField] private Transform firePointRight1;
    [SerializeField] private Transform firePointRight2;
    [SerializeField] private float fireCooldown = 1f;
    [SerializeField] private AudioClip laserSound;

    [Header("Drop")]
    [SerializeField] private EnemyCoinDrop coinDrop;

    private Animator animator;
    private Health health;
    private Transform player;
    private bool isRevealed, isDead;
    private float fireTimer;
    private int currentBarrel;
    private AudioSource audioSource;

    void Awake()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        health.onDamageTaken.AddListener(_ => animator.SetTrigger("Damage"));
        health.onDeath.AddListener(OnDeath);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.volume = 0.7f;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (!isRevealed)
        {
            var hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
            if (hit != null && hit.CompareTag("Player"))
            {
                player = hit.transform;
                isRevealed = true;
                animator.SetTrigger("Reveal");
            }
            return;
        }

        fireTimer += Time.deltaTime;
        AimAtPlayer();

        if (fireTimer >= fireCooldown)
        {
            fireTimer = 0f;
            animator.SetBool("IsShooting", true);
        }
    }

    public void LaserSound()
    {
        if (laserSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(laserSound);
        }
    }

    void AimAtPlayer()
    {
        if (player == null) return;
        Vector2 d = (player.position - transform.position).normalized;
        float x = Mathf.Abs(d.x) > Mathf.Abs(d.y) ? Mathf.Sign(d.x) : 0f;
        float y = Mathf.Abs(d.y) > Mathf.Abs(d.x) ? Mathf.Sign(d.y) : 0f;
        animator.SetFloat("Horizontal", x);
        animator.SetFloat("Vertical", y);
    }

    public void Fire1() => FireFromBarrel(1);
    public void Fire2()
    {
        FireFromBarrel(2);
        animator.SetBool("IsShooting", false);
    }

    void FireFromBarrel(int barrel)
    {
        if (isDead || player == null) return;

        float x = animator.GetFloat("Horizontal"), y = animator.GetFloat("Vertical");
        Transform fp = null;

        if (y > 0) fp = barrel == 1 ? firePointUp1 : firePointUp2;
        else if (y < 0) fp = barrel == 1 ? firePointDown1 : firePointDown2;
        else if (x > 0) fp = barrel == 1 ? firePointRight1 : firePointRight2;
        else if (x < 0) fp = barrel == 1 ? firePointLeft1 : firePointLeft2;

        if (fp == null || projectilePrefab == null) return;

        var projGO = Instantiate(projectilePrefab, fp.position, Quaternion.identity);
        if (projGO.TryGetComponent<BulletOfTurret>(out var b))
            b.direction = new Vector2(x, y).normalized; 
    }

    void OnDeath()
    {
        isDead = true;
        animator.SetTrigger("Die");

        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
    }

    public void DestroySelf()
    {
        if (coinDrop != null) coinDrop.DropItems();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}