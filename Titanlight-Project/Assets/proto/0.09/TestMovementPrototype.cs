using UnityEngine;
using System.Collections;

public class TestMovementPrototype : MonoBehaviour
{
    [Header("Configurações de Hack")]
    public CountdownTimer timer;
    public float hackTimeReduction = 10f;
    public int maxHacks = 3;
    public LayerMask doorLayer;

    [Header("Configurações")]
    public float moveSpeed = 5f;

    private Rigidbody2D _rb;
    private int hacksRemaining;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        hacksRemaining = maxHacks;
    }

    private void Update()
    {
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.H))
        {
            TryHackDoor();
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        _rb.linearVelocity = new Vector2(moveX, moveY).normalized * moveSpeed;
    }

    void TryHackDoor()
    {
        DoorTrigger targetDoor = GetTargetDoor();

        if (CanHackDoor(targetDoor))
        {
            PerformHack(targetDoor);
        }
    }

    bool CanHackDoor(DoorTrigger targetDoor)
    {
        return targetDoor != null &&
               hacksRemaining > 0 &&
               timer != null &&
               targetDoor.State.isLocked;
    }

    void PerformHack(DoorTrigger targetDoor)
    {
        timer.ReduceTime(hackTimeReduction);
        hacksRemaining--;
        targetDoor.UnlockDoor();
        Debug.Log("Hack realizado! Hacks restantes: " + hacksRemaining);
        StartCoroutine(FlashTimerColor());
    }

    DoorTrigger GetTargetDoor()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            _rb.linearVelocity.normalized,
            1.5f,
            doorLayer
        );
        return hit.collider?.GetComponent<DoorTrigger>();
    }

    IEnumerator FlashTimerColor()
    {
        if (timer.timerText != null)
        {
            Color originalColor = timer.timerText.color;
            timer.timerText.color = Color.green;
            yield return new WaitForSeconds(0.5f);
            timer.timerText.color = originalColor;
        }
    }
}