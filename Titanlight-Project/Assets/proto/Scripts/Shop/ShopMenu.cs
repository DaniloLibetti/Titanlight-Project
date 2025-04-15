using UnityEngine;
using UnityEngine.UI;
using Inventory.Model;
using TMPro;
using System.Collections.Generic;

namespace Inventory.UI
{
    public class ShopMenu : MonoBehaviour
    {
        public TextMeshProUGUI coinsText;
        [SerializeField] private GameObject shopMenu;

        private int coins;
        private ItemSO itemSO;

        int itemIndex;
        int amount;

        public void SellButton()
        {

        }

        public void SellItems()
        {
            
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            coinsText.text = "Coins:" + coins;
        }

        public void Hide() //desativa a interface do inventario
        {
            //actionPanel.Toggle(false);
            shopMenu.SetActive(false);
            //ResetDraggedItem();
        }

        public void Show() //ativa a interface do inventario
        {
            shopMenu.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (!shopMenu.activeSelf)
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
}