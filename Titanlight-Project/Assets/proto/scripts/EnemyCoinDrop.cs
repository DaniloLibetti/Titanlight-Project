using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyCoinDrop : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private List<ItemSO> possibleDrops = new List<ItemSO>();
    [SerializeField] private int minDrops = 1;
    [SerializeField] private int maxDrops = 3;
    [SerializeField] private float scatterRadius = 1f;

    [Header("Physics Settings")]
    [SerializeField] private float minImpulse = 2f;
    [SerializeField] private float maxImpulse = 4f;

    private void Start()
    {
        GetComponent<Health>().onDeath.AddListener(DropItems);
    }

    public void DropItems()
    {
        int count = Random.Range(minDrops, maxDrops + 1);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = transform.position + (Vector3)Random.insideUnitCircle * scatterRadius;
            GameObject item = Instantiate(dropPrefab, pos, Quaternion.identity);

            Item itemComp = item.GetComponent<Item>();
            itemComp.Init(possibleDrops[Random.Range(0, possibleDrops.Count)]);

            ApplyDropForce(item);
        }
    }

    private void ApplyDropForce(GameObject item)
    {
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 force = Random.insideUnitCircle.normalized * Random.Range(minImpulse, maxImpulse);
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
}