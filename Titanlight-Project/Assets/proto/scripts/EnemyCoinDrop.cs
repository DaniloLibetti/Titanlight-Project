using UnityEngine;

public class EnemyCoinDrop : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float dropOffset = 1f;

    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
        if (health != null)
            health.onDeath.AddListener(DropCoins);
        else
            Debug.LogWarning($"Health component not found on {gameObject.name}");
    }

    public void DropCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogError("coinPrefab não está atribuído!");
            return;
        }

        Vector3 pos = transform.position;
        Vector3 offset1 = new Vector3(dropOffset, 0, 0);
        Vector3 offset2 = new Vector3(-dropOffset, 0, 0);

        Instantiate(coinPrefab, pos + offset1, Quaternion.identity);
        Instantiate(coinPrefab, pos + offset2, Quaternion.identity);
    }
}
