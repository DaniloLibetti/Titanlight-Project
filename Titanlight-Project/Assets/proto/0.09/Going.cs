using UnityEngine;

public class Going : MonoBehaviour
{
    public float distance = 5f;
    public float gizmoSize = 1f;
    public bool isHorizontal = true; // Se falso, será vertical

    private Vector2 point1;
    private Vector2 point2;
    private CircleCollider2D collider1;
    private CircleCollider2D collider2;
    private bool playerInside = false; // Verifica se o player está dentro

    void Start()
    {
        CalculatePoints();

        // Adiciona e configura os CircleColliders2D
        collider1 = gameObject.AddComponent<CircleCollider2D>();
        collider1.isTrigger = true;
        collider1.radius = gizmoSize;
        collider1.offset = transform.InverseTransformPoint(point1);

        collider2 = gameObject.AddComponent<CircleCollider2D>();
        collider2.isTrigger = true;
        collider2.radius = gizmoSize;
        collider2.offset = transform.InverseTransformPoint(point2);
    }

    void CalculatePoints()
    {
        if (isHorizontal)
        {
            point1 = (Vector2)transform.position + Vector2.left * distance;
            point2 = (Vector2)transform.position + Vector2.right * distance;
        }
        else
        {
            point1 = (Vector2)transform.position + Vector2.up * distance;
            point2 = (Vector2)transform.position + Vector2.down * distance;
        }
    }

    public bool CanTeleport() => playerInside;

    public Vector2 GetOtherPoint(Vector2 currentPos)
    {
        if (Vector2.Distance(currentPos, point1) < gizmoSize) return point2;
        if (Vector2.Distance(currentPos, point2) < gizmoSize) return point1;
        return currentPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    void OnDrawGizmos()
    {
        CalculatePoints();

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(point1, gizmoSize);
        Gizmos.DrawWireSphere(point2, gizmoSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(point1, point2);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(point1, gizmoSize * 0.5f);
        Gizmos.DrawSphere(point2, gizmoSize * 0.5f);
    }
}
