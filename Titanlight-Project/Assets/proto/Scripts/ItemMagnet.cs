using UnityEngine;

public class ItemMagnet : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Item>(out Item item))
        {
            item.SetTarget(transform.parent.position);
        }
    }
}
