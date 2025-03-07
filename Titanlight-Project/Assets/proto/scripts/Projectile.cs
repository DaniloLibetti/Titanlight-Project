using UnityEngine;

public class Projectile : MonoBehaviour
{
    public PlayerController playerController;
    public AreaLimiter areaLimiter;

    private Vector2 direction;
    private float speed;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearVelocity = direction * speed; // Usando rb.velocity
    }

    void Update()
    {
        if (areaLimiter != null)
        {
            // Verifica se o projétil está fora dos limites
            if (IsOutOfBounds(rb.position))
            {
                Destroy(gameObject); // Destroi o projétil se estiver fora dos limites
                return;
            }

            // Mantém o projétil dentro dos limites
            Vector2 clampedPosition = areaLimiter.ClampPosition(rb.position);
            rb.position = clampedPosition;
        }
    }

    // Verifica se o projétil está fora dos limites
    private bool IsOutOfBounds(Vector2 position)
    {
        return position.x < areaLimiter.MinBounds.x || position.x > areaLimiter.MaxBounds.x ||
               position.y < areaLimiter.MinBounds.y || position.y > areaLimiter.MaxBounds.y;
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir;
        if (rb != null)
            rb.linearVelocity = direction * speed; // Usando rb.velocity
    }

    public void SetSpeed(float projectileSpeed)
    {
        speed = projectileSpeed;
        if (rb != null)
            rb.linearVelocity = direction * speed; // Usando rb.velocity
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Projétil colidiu com: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(10);
            }
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Debug.Log("Projétil destruído!");
        if (playerController != null)
        {
            playerController.ProjectileDestroyed();
        }
    }
}
