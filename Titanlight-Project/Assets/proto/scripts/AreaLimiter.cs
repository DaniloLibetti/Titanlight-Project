using UnityEngine;

public class AreaLimiter : MonoBehaviour
{
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;
    [SerializeField] private float boundaryBuffer = 0.1f;

    public Vector2 MinBounds => minBounds;
    public Vector2 MaxBounds => maxBounds;

    public Vector2 ClampPosition(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, minBounds.x + boundaryBuffer, maxBounds.x - boundaryBuffer),
            Mathf.Clamp(position.y, minBounds.y + boundaryBuffer, maxBounds.y - boundaryBuffer)
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((minBounds + maxBounds) / 2, maxBounds - minBounds);
    }
}