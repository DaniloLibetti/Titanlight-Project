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
    private bool _isPlayerInRange;

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
            // Movimento suave em direção ao jogador
            Vector2 direction = (_player.position - transform.position).normalized;
            _rb.linearVelocity = direction * attractionSpeed;

            // Se chegarmos perto o suficiente, completar coleta
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
        if (!_readyToCollect) return;

        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
            GameUI.Instance.ToggleInteractionText(true); // exibe “Pressione [Interact] para coletar”
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            GameUI.Instance.ToggleInteractionText(false);
        }
    }

    /// <summary>
    /// Chamado pelo PlayerStateMachine quando o jogador aperta o botão Interact
    /// e este item está no alcance (_isPlayerInRange == true).
    /// </summary>
    public void Interact()
    {
        if (!_isPlayerInRange || !_readyToCollect)
            return;

        StartAttraction(_player);
    }

    private void StartAttraction(Transform player)
    {
        _player = player;
        _isAttracting = true;

        // Anula todas as forças físicas para começar a atrair
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.gravityScale = 0f;

        GameUI.Instance.ToggleInteractionText(false);
    }

    private void Collect()
    {
        if (_player == null) return;

        PickUpSystem pickupSystem = _player.GetComponent<PickUpSystem>();
        if (pickupSystem != null)
        {
            pickupSystem.AddItem(InventoryItem, Quantity);
        }
        Destroy(gameObject);
    }
}
