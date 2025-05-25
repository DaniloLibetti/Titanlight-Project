using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float attractionSpeed = 8f;
    [SerializeField] private float collectDistance = 0.3f;
    [SerializeField] private float activationDelay = 0.5f;

    public ItemSO InventoryItem { get; private set; }
    public int Quantity { get; private set; }

    private Rigidbody2D _rb;
    private Transform _player;
    private bool _isAttracting;
    private bool _readyToCollect;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        GetComponent<Collider2D>().enabled = false;
    }

    private void Start()
    {
        Invoke(nameof(EnableCollection), activationDelay);
    }

    private void EnableCollection()
    {
        GetComponent<Collider2D>().enabled = true;
        _readyToCollect = true;
    }

    private void FixedUpdate()
    {
        if (_isAttracting && _player != null)
        {
            // Movimento suave usando física
            Vector2 direction = (_player.position - transform.position).normalized;
            _rb.linearVelocity = direction * attractionSpeed;

            // Verifica distância para coleta
            if (Vector2.Distance(transform.position, _player.position) <= collectDistance)
            {
                Collect();
            }
        }
    }

    public void Init(ItemSO so, int quantity = 1)
    {
        InventoryItem = so;
        Quantity = quantity;
        GetComponent<SpriteRenderer>().sprite = so.ItemImage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_readyToCollect || _isAttracting) return;

        if (other.CompareTag("Player"))
        {
            StartAttraction(other.transform);
        }
    }

    private void StartAttraction(Transform player)
    {
        _player = player;
        _isAttracting = true;

        // Para todas as forças físicas
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.gravityScale = 0f;
    }

    private void Collect()
    {
        PickUpSystem pickupSystem = _player.GetComponent<PickUpSystem>();
        if (pickupSystem != null)
        {
            pickupSystem.AddItem(InventoryItem, Quantity);
        }
        Destroy(gameObject);
    }
}