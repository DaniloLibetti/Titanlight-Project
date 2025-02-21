using UnityEngine;

public class areaLimiter : MonoBehaviour
{
    [Header("Área Limitada")]
    // Canto inferior esquerdo
    [SerializeField] private Vector2 minBounds;
    // Canto superior direito  
    [SerializeField] private Vector2 maxBounds; 

    public Vector2 MinBounds => minBounds;
    public Vector2 MaxBounds => maxBounds;
}