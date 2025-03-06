using UnityEngine;
using UnityEngine.UI;

public class Ui_Inventory : MonoBehaviour
{
    private Inventory inventory;
    [SerializeField] private Transform itemSlotContainer;
    [SerializeField] private Transform itemSlotTemplate;

    /*private void Awake()
    {
        itemSlotContainer = transform.Find("itemSlotContainer");    
        itemSlotTemplate = transform.Find("itemSlotTemplate");
    }*/

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;
        RefreshInventoryItens();
    }

    private void RefreshInventoryItens()
    {
        int x = 0;
        int y = 0;
        float itemSlotCellSize = 30f;

        foreach(Itens item in inventory.GetItemList())
        {
          RectTransform itemSlotRectTransform = Instantiate(itemSlotTemplate, itemSlotContainer).GetComponent<RectTransform>();
          itemSlotRectTransform.gameObject.SetActive(true);

          itemSlotRectTransform.anchoredPosition = new Vector2(x + itemSlotCellSize, y + itemSlotCellSize);
          Image image = itemSlotRectTransform.Find("image").GetComponent<Image>();
          image.sprite = item.GetSprite();
          x++;
          if(x > 4)
            {
                x = 0;
                y++;
            }
          
        }
    }

}
