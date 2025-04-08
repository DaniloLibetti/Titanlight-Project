using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private LayerMask collisionLayers;
    private PlayerController playerController;

    // Campo para multiplicador de dano
    public float damageMultiplier = 1f;

    // Tempo de vida configurável para cada prefab
    [SerializeField] private float lifetime = 0.5f;

    public void Initialize(Vector2 newDirection, float newSpeed, LayerMask layers, PlayerController controller)
    {
        direction = newDirection.normalized;
        speed = newSpeed;
        collisionLayers = layers;
        playerController = controller;
        RotateProjectile();
    }

    void Start()
    {
        // Destroi o projétil após o tempo definido no prefab
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void RotateProjectile()
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if ((collisionLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            if (other.CompareTag("Enemy"))
            {
                Health health = other.GetComponent<Health>();
                if (health != null)
                {
                    // Aplica o dano base multiplicado pelo damageMultiplier
                    health.TakeDamage(10 * damageMultiplier);
                }
            }
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        if (playerController != null)
        {
            playerController.ProjectileDestroyed();
        }
        Destroy(gameObject);
    }
}
