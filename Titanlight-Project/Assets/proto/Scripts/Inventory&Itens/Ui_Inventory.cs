using UnityEngine;

public class Ui_Inventory : MonoBehaviour
{
    private Inventory inventory;
    private Transform itemSlotContainer;
    private Transform itemSlotTemplate;

    private void Awake()
    {
        itemSlotContainer = transform.Find("itemSlotContainer");
        itemSlotTemplate = transform.Find("itemSlotTemplate");
    }

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
          x++;
          if(x > 4)
            {
                x = 0;
                y++;
            }
          
        }
    }

}
