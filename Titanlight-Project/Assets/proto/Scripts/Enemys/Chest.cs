using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Chest : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Opening";
    [SerializeField] private string openStateParam = "Aberto";

    [Header("Drop System")]
    [SerializeField] private ChestDrop dropSystem;

    private bool isOpened = false;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void OpenChest()
    {
        if (isOpened) return;

        isOpened = true;
        animator.SetTrigger(openTrigger);

        if (dropSystem != null)
        {
            dropSystem.DropItems();
        }
    }

    public void OnOpenAnimationComplete()
    {
        animator.SetBool(openStateParam, true);
    }

    public bool IsChestOpened()
    {
        return isOpened;
    }
}