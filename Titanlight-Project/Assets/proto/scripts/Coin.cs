using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Configurações da Moeda")]
    public int value = 1; // quantas moedas vale (1 padrão)  
    public float pickupRange = 2f; // distância que atrai pro player  
    public float attractionSpeed = 5f; // velocidade de movimento  

    private Transform playerTransform;
    private bool isAttracting = false; // tá sendo atraída?  

    void Start()
    {
        // Busca o player pela tag (certifique que o player tem a tag certa)  
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
            Debug.LogWarning("Player não encontrado para atrair a moeda!");
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // Verifica distância do player  
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance <= pickupRange)
                isAttracting = true; // ativa atração se tá dentro do alcance  
        }

        // Move a moeda suavemente até o player  
        if (isAttracting && playerTransform != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, attractionSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // Se colidiu com player, adiciona valor e destroi moeda  
        if (col.CompareTag("Player") && GameManager.Instance != null)
        {
            GameManager.Instance.RegisterScriptableObject(value); // atualiza contador  
            Destroy(gameObject);  
        }
    }
}