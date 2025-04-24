using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private LayerMask collisionLayers;

    // Multiplicador de dano
    public float damageMultiplier = 1f;

    // Tempo de vida configurável no prefab
    [SerializeField] private float lifetime = 0.5f;

    /// <summary>
    /// Overload para manter compatibilidade com
    /// PlayerController.Initialize(..., controller)
    /// </summary>
    public void Initialize(Vector2 newDirection, float newSpeed, LayerMask layers, PlayerController controller)
    {
        Initialize(newDirection, newSpeed, layers);
    }

    /// <summary>
    /// Inicializa direção, velocidade e camadas de colisão.
    /// </summary>
    public void Initialize(Vector2 newDirection, float newSpeed, LayerMask layers)
    {
        direction = newDirection.normalized;
        speed = newSpeed;
        collisionLayers = layers;
        RotateProjectile();
    }

    void Start()
    {
        // Destroi o projétil após 'lifetime' segundos
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move o projétil
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void RotateProjectile()
    {
        // Ajusta a rota��o para a direção do movimento
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se colidiu com uma layer válida
        if ((collisionLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            if (other.CompareTag("Enemy"))
            {
                var health = other.GetComponent<Health>();
                if (health != null)
                    health.TakeDamage(10 * damageMultiplier);
            }
            // Destroi o projétil imediatamente
            Destroy(gameObject);
        }
    }
}
