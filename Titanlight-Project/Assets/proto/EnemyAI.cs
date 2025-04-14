using UnityEngine;

/*public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Health health;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (health == null) health = GetComponent<Health>();
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<Controle>(out Controle playerController))
            {
                //playerController.TakeDamage(damage);
            }
        }
    }
}*/