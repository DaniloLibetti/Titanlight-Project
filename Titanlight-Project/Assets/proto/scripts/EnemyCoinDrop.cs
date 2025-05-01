using UnityEngine;

public class EnemyCoinDrop : MonoBehaviour
{
    [Header("Drop Settings")] // Configs do que dropa  
    [Tooltip("Prefab do objeto que será dropado")]
    [SerializeField] private GameObject dropPrefab; // item q aparece qnd o inimigo morre  
    [Tooltip("Quantidade de objetos a serem contabilizados ao coletar")]
    [SerializeField] private int dropAmount = 1; // qtd de itens (ex: +1)  

    private Health health; // referencia pro componente de vida  

    void Start()
    {
        health = GetComponent<Health>(); // pega o Health do inimigo  
        if (health != null)
            health.onDeath.AddListener(DropItem); // qnd morrer, droppa o item  
        else
            Debug.LogWarning($"Health component not found on {gameObject.name}"); // avisa se não tiver health  
    }

    private void DropItem() // executa qnd o inimigo morre  
    {
        if (dropPrefab == null)
        {
            Debug.LogError("Drop Prefab não está atribuído em EnemyCoinDrop!"); // erro se faltar prefab  
            return;
        }

        // Cria o item no chão na posição do inimigo  
        GameObject drop = Instantiate(dropPrefab, transform.position, Quaternion.identity);

        // Configura a qtd no script de coleta (PickupableItem)  
        var pickup = drop.GetComponent<PickupableItem>();
        if (pickup != null)
            pickup.SetAmount(dropAmount); // define qts itens vale  
        else
            Debug.LogWarning("Prefab dropado precisa do componente PickupableItem!"); // avisa se faltar componente  
    }
}