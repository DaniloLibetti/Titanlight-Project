using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Configurações da Moeda")]
    [Tooltip("Valor que essa moeda representa")]
    public int value = 1;

    [Tooltip("Distância para iniciar a atração do jogador")]
    public float pickupRange = 2f;

    [Tooltip("Velocidade com que a moeda se move em direção ao jogador")]
    public float attractionSpeed = 5f;

    private Transform playerTransform;
    private bool isAttracting = false;

    void Start()
    {
        // Procura o jogador utilizando a tag "Player"
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
            Debug.LogWarning("Player não encontrado para atrair a moeda!");
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance <= pickupRange)
            {
                isAttracting = true;
            }
        }

        if (isAttracting && playerTransform != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, attractionSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Debug.Log("Moeda colidiu com o Player!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterCoin(value);
            }
            Destroy(gameObject);
        }
    }
}
