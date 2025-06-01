// PickupableItem.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class PickupableItem : MonoBehaviour
{
    [Tooltip("Distância máxima para interagir")]
    public float interactRange = 1.5f;
    [Tooltip("Texto de dica (coloque um TextMeshPro no mundo ou na UI)")]
    public TextMeshProUGUI promptText;

    private int amount = 1;
    private Transform player;
    private bool inRange = false;
    private bool promptVisible = false;
    private bool _readyToCollect = false;
    [SerializeField] private float activationDelay = 0.5f;

    private Rigidbody2D _rb;
    private bool _isAttracting;

    public ItemSO InventoryItem { get; private set; }
    public int Quantity { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        GetComponent<Collider2D>().enabled = false;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
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
        if (_isAttracting && player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            _rb.linearVelocity = direction * interactRange;

            if (Vector2.Distance(transform.position, player.position) <= 0.3f)
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
            Debug.Log("Player entrou no alcance do item " + gameObject.name);
            inRange = true;
            ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player saiu do alcance do item " + gameObject.name);
            inRange = false;
            HidePrompt();
        }
    }

    public void Interact()
    {
        Debug.Log("PickupableItem.Interact chamado em " + gameObject.name);
        if (!inRange || !_readyToCollect)
            return;

        StartAttraction(player);
    }

    private void StartAttraction(Transform plyr)
    {
        _isAttracting = true;
        player = plyr;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.gravityScale = 0f;

        HidePrompt();
    }

    private void Collect()
    {
        Debug.Log("Item coletado: " + gameObject.name);
        PickUpSystem pickupSystem = player.GetComponent<PickUpSystem>();
        if (pickupSystem != null)
        {
            pickupSystem.AddItem(InventoryItem, Quantity);
        }
        Destroy(gameObject);
    }

    public void SetAmount(int amt)
    {
        amount = amt;
    }

    private void ShowPrompt()
    {
        if (promptText != null && !promptVisible)
        {
            promptText.text = "Pressione [Interact] para coletar";
            promptText.gameObject.SetActive(true);
            promptVisible = true;
        }
    }

    private void HidePrompt()
    {
        if (promptText != null && promptVisible)
        {
            promptText.gameObject.SetActive(false);
            promptVisible = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
