using UnityEngine;
using UnityEngine.UI;
using Inventory.UI;
using Inventory.Model;
using TMPro;

public class ShopMenu : MonoBehaviour
{
    public TextMeshProUGUI coinsText;
    [SerializeField] ShopMenu shopMenu;

    private int coins;
    private ItemSO itemSO;
    private InventoryUi inventoryUi;


    public void SellButton()
    {
        inventoryUi.ResetAllItems();
        SellItems();
    }

    public void SellItems()
    {
        coins += itemSO.SellValue;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        coinsText.text = "Coins:" + coins;
    }

    public void Hide() //desativa a interface do inventario
    {
        //actionPanel.Toggle(false);
        gameObject.SetActive(false);
        //ResetDraggedItem();
    }

    public void Show() //ativa a interface do inventario
    {
        gameObject.SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (shopMenu.isActiveAndEnabled == false)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}
