// PickupableItem.cs
using UnityEngine;
using TMPro;
using Player.StateMachine; // para escutar InteractPressed

[RequireComponent(typeof(Collider2D))]
public class PickupableItem : MonoBehaviour
{
    [Tooltip("Distância máxima para interagir")]
    public float interactRange = 1.5f;
    [Tooltip("Texto de dica (coloque um TextMeshPro no mundo ou na UI)")]
    public TextMeshProUGUI promptText;

    private Transform _playerTransform;
    private bool _inRange = false;
    private bool _promptVisible = false;
    private bool _readyToCollect = false;
    [SerializeField] private float activationDelay = 0.5f;

    private Rigidbody2D _rb;
    private bool _isAttracting;

    public ItemSO InventoryItem { get; private set; }
    public int Quantity { get; private set; }

    void OnEnable()
    {
        // Agora o delegate exige um método Action<PlayerStateMachine>
        PlayerStateMachine.InteractPressed += HandleInteractPressed;
    }

    void OnDisable()
    {
        PlayerStateMachine.InteractPressed -= HandleInteractPressed;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        GetComponent<Collider2D>().enabled = false;
    }

    private void Start()
    {
        // Busca o transform do jogador (tag "Player")
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            _playerTransform = playerGO.transform;

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        Invoke(nameof(EnableCollection), activationDelay);
    }

    private void EnableCollection()
    {
        GetComponent<Collider2D>().enabled = true;
        _readyToCollect = true;
    }

    private void FixedUpdate()
    {
        if (_isAttracting && _playerTransform != null)
        {
            Vector2 direction = (_playerTransform.position - transform.position).normalized;
            _rb.linearVelocity = direction * interactRange;

            if (Vector2.Distance(transform.position, _playerTransform.position) <= 0.3f)
                Collect();
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
            _inRange = true;
            ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _inRange = false;
            HidePrompt();
        }
    }

    // Agora recebe o PlayerStateMachine que disparou o evento
    private void HandleInteractPressed(PlayerStateMachine player)
    {
        // Garante que é o mesmo jogador que está no trigger
        if (!_inRange || !_readyToCollect || player.transform != _playerTransform)
            return;

        StartAttraction(_playerTransform);
    }

    private void StartAttraction(Transform player)
    {
        _isAttracting = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.gravityScale = 0f;

        HidePrompt();
    }

    private void Collect()
    {
        var pickupSystem = _playerTransform.GetComponent<PickUpSystem>();
        if (pickupSystem != null)
            pickupSystem.AddItem(InventoryItem, Quantity);

        Destroy(gameObject);
    }

    public void SetAmount(int amt)
    {
        Quantity = amt;
    }

    private void ShowPrompt()
    {
        if (promptText != null && !_promptVisible)
        {
            promptText.text = "Pressione [Interact] para coletar";
            promptText.gameObject.SetActive(true);
            _promptVisible = true;
        }
    }

    private void HidePrompt()
    {
        if (promptText != null && _promptVisible)
        {
            promptText.gameObject.SetActive(false);
            _promptVisible = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
