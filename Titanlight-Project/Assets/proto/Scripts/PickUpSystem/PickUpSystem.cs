using Inventory.Model;
using UnityEngine;

public class PickUpSystem : MonoBehaviour
{
    [SerializeField]
    private InventoryData inventoryData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();
        if (item != null)
        {
            int reminder = inventoryData.AddItem(item.InventoryItem, item.Quantity, item.itemType);
            if(reminder == 0)
            {
                item.DestroyItem();
            }
            else
            {
                item.Quantity = reminder;
            }
        }
    }
}
