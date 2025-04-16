
using TMPro;
using UnityEngine;

public class PickUpSystem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI itemsPickedText;

    private int itemsCount = 0;

    private void Start()
    {
        itemsPickedText.text = "Espólios: " + itemsCount;
    }

    public void AddItem()
    {
        itemsCount++;
        itemsPickedText.text = "Espólios: " + itemsCount;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();
        if (item != null)
        {
            //int reminder = itemPicked.AddItem
            AddItem();
            item.DestroyItem();
            
        }
    }

}
