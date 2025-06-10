using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BulletOfTurret : MonoBehaviour
{
    [Header("Configurações do Projétil")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 4f;
    [SerializeField] private float lifeTime = 5f;

    private Rigidbody2D rb;

    private SegmentedHealthBar healthBar;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);

        healthBar = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<SegmentedHealthBar>();
    }

    /// <summary>
    /// Chamado pela torreta para definir direção e partida do projétil.
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * speed;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Se for o player, aplica dano
        if (other.CompareTag("Player"))
        {
            var hp = other.GetComponent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                healthBar.SetValue(damage);
            }
                
        }

        // Em qualquer colisão (player, parede, objetos), destrói o projétil
        Destroy(gameObject);
    }
}
