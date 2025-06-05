using UnityEngine;

public class ItemCollect : MonoBehaviour
{
    private Item item;
    private void Start()
    {
        item = GetComponent<Item>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item groundItem = collision.GetComponent<Item>();
        if(groundItem != null)
        {
            groundItem.Collect();
        }
    }
}
