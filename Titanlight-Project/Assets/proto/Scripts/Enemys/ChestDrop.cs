using System.Collections.Generic;
using UnityEngine;

public class ChestDrop : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private List<ItemSO> possibleDrops = new List<ItemSO>();
    [SerializeField] private int minDrops = 1;
    [SerializeField] private int maxDrops = 3;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] dropPoints; // 6 pontos ao redor do baú

    [Header("Physics Settings")]
    [SerializeField] private float minImpulse = 2f;
    [SerializeField] private float maxImpulse = 4f;
    [SerializeField] private float upwardForce = 3f; // Força vertical adicional

    public void DropItems()
    {
        if (dropPrefab == null)
        {
            Debug.LogError("Drop prefab is not assigned!");
            return;
        }

        if (possibleDrops == null || possibleDrops.Count == 0)
        {
            Debug.LogError("No possible drops assigned!");
            return;
        }

        if (dropPoints == null || dropPoints.Length == 0)
        {
            Debug.LogError("No drop points assigned!");
            return;
        }

        int count = Mathf.Clamp(Random.Range(minDrops, maxDrops + 1), 1, dropPoints.Length);

        // Embaralhar os pontos para seleção aleatória
        List<Transform> shuffledPoints = new List<Transform>(dropPoints);
        Shuffle(shuffledPoints);

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = shuffledPoints[i];
            GameObject item = Instantiate(dropPrefab, spawnPoint.position, Quaternion.identity);

            Item itemComp = item.GetComponent<Item>();
            if (itemComp != null && possibleDrops.Count > 0)
            {
                int randomIndex = Random.Range(0, possibleDrops.Count);
                if (possibleDrops[randomIndex] != null)
                {
                    itemComp.Init(possibleDrops[randomIndex]);
                }
            }

            ApplyDropForce(item, spawnPoint.position);
        }
    }

    private void ApplyDropForce(GameObject item, Vector3 spawnPosition)
    {
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Direção radial para fora do baú
            Vector2 direction = (item.transform.position - transform.position).normalized;

            // Adicionar componente vertical
            Vector2 force = (direction + Vector2.up * 0.5f).normalized * Random.Range(minImpulse, maxImpulse);

            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    // Método para embaralhar pontos
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Mostra os pontos no editor
    private void OnDrawGizmosSelected()
    {
        if (dropPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in dropPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawSphere(point.position, 0.1f);
                    Gizmos.DrawLine(transform.position, point.position);
                }
            }
        }
    }
}