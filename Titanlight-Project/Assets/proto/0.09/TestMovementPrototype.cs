using UnityEngine;

public class TestMovementPrototype : MonoBehaviour
{
    [Header("Configurações")]
    public float moveSpeed = 5f; // Velocidade de movimento

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveX, moveY).normalized;
        _rb.linearVelocity = movement * moveSpeed;
    }
}
