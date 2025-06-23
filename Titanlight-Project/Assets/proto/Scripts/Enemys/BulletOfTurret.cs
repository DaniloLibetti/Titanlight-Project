using UnityEngine;

public class BulletOfTurret : MonoBehaviour
{
    public float damage = 4f;
    public float speed = 10f;
    public Vector2 direction = Vector2.right;

    private Rigidbody2D _rb;

    private SegmentedHealthBar healthBar;

    private void Awake()
    {
        healthBar = GetComponent<SegmentedHealthBar>();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.linearVelocity = direction * speed;

        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                //healthBar.UpdateHealthBar(damage);
            }
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy") && !other.CompareTag("Projectile"))
        {
            Destroy(gameObject);
        }
    }
}