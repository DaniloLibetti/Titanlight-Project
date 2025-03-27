using UnityEngine;

public class TestMovementPrototype : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        _rb.linearVelocity = new Vector2(moveX, moveY).normalized * moveSpeed;
    }
}
