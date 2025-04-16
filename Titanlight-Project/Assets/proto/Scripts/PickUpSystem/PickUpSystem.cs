
using UnityEngine;

public class PickUpSystem : MonoBehaviour
{
    [SerializeField]
    private Item itemPicked;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();
        if (item != null)
        {
            //int reminder = itemPicked.AddItem
            item.DestroyItem();
            itemPicked.AddItem();
        }
    }

}
