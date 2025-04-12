using UnityEngine;
using UnityEngine.Events;

public class EnemyCoinDrop : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float dropOffset = 1f; // deslocamento para posicionar as moedas

    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            // Se inscreve no evento onDropMoeda do Health
            health.onDropMoeda.AddListener(DropCoins);
        }
        else
        {
            Debug.LogWarning("Health component not found on " + gameObject.name);
        }
    }

    public void DropCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogError("coinPrefab não está atribuído!");
            return;
        }

        // Pega a posição do inimigo
        Vector3 pos = transform.position;

        // Calcula os offsets para cada moeda
        Vector3 offset1 = new Vector3(dropOffset, 0, 0);
        Vector3 offset2 = new Vector3(-dropOffset, 0, 0);

        // Instancia as duas moedas
        Instantiate(coinPrefab, pos + offset1, Quaternion.identity);
        Instantiate(coinPrefab, pos + offset2, Quaternion.identity);
    }
}
