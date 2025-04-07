using Inventory.Model;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace Inventory.UI
{
    public class InventoryUi : MonoBehaviour
    {
        [SerializeField] private UIInventoryItem itemPrefab;//prefab do item na UI
        [SerializeField] private RectTransform contentPanel;//painel aonde fica os itens
        //[SerializeField] private MouseFollowe mouseFollower;//sprite do item q esta sendo arrastado
        [SerializeField] private RectTransform equipmentPanel;//painel aonde fica os equipamentos
        [SerializeField] private EquipmentSlot meleeSlot, rangeSlot, upgradeSlot1, upgradeSlot2, throwableSlot, consumableSlot;

        public bool isItemSelected = false;

        List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();//lista do q esta no inventario do jogador

        //private int currentlyDraggedItemIndex = -1;//index do item q esta sendo arrastado

        public event Action<int> OnItemSelection; //OnStartDragging 

        //public event Action<int, int> OnSwapItems;

        //[SerializeField] private ItemActionPanel actionPanel;//painel de açoes do item

        private ItemType itemType;
        private Sprite itemImage;
        private int itemIndex;

        private void Awake()
        {
            Hide();
            //mouseFollower.Toggle(false);
        }

        public void InitializeInventoryUI(int inventorysize)
        {
            for (int i = 0; i < inventorysize; i++)
            {
                UIInventoryItem uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                uiItem.transform.SetParent(contentPanel);
                uiItem.transform.localScale = Vector3.one;
                listOfUIItems.Add(uiItem);
                uiItem.OnItemClicked += HandleItemSelection;
                //uiItem.OnItemBeginDrag += HandleBeginDrag;
                //uiItem.OnItemDroppedOn += HandleSwap;
                //uiItem.OnItemEndDrag += HandleEndDrag;
                //uiItem.OnRightMouseButtonClick += HandleShowItemActions;
            }
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity, ItemType itemType) //atualiza a informaçao do item
        {
            if (listOfUIItems.Count > itemIndex)
            {
                listOfUIItems[itemIndex].SetData(itemImage, itemQuantity, itemType);
            }
        }

        /*private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
        }*/

        /*private void HandleEndDrag(UIInventoryItem inventoryItemUI)
        {
            mouseFollower.Toggle(false);
        }*/

        /*private void HandleSwap(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
        }*/

        /*private void ResetDraggedItem()
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }*/

        /*private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            currentlyDraggedItemIndex = index;
            HandleItemSelection(inventoryItemUI);
            OnStartDragging?.Invoke(index);
        }*/

        /*public void CreateDraggedItem(Sprite sprite, int quantity, ItemType itemType)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, quantity, itemType);
        }*/

        private void HandleItemSelection(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            DeselectAllItems();
            listOfUIItems[index].Select();
            isItemSelected = true;
            OnItemSelection?.Invoke(index);
        }   

        public void Show() //ativa a interface do inventario
        {
            gameObject.SetActive(true);
            ResetSelection();
        }

        public void ResetSelection() //reseta a seleçao de itens
        {
            DeselectAllItems();
        }

        private void DeselectAllItems() //desseleciona todos os itens
        {
            foreach (UIInventoryItem item in listOfUIItems)
            {
                item.Deselect();
            }
            //actionPanel.Toggle(false);
        }

        /*public void AddAction(string actionName, Action performAction)
        {
            actionPanel.AddButon(actionName, performAction);
        }*/

        /*public void ShowItemAction(int itemIndex) //mostra o painel de açoes do item
        {
            actionPanel.Toggle(true);
            actionPanel.transform.position = listOfUIItems[itemIndex].transform.position;
        }*/

        public void Hide() //desativa a interface do inventario
        {
            //actionPanel.Toggle(false);
            gameObject.SetActive(false);
            //ResetDraggedItem();
        }

        public void ResetAllItems() //reseta todos os itens
        {
            foreach (var item in listOfUIItems)
            {
                item.ResetData();
                item.Deselect();
            }    
        }



    }

    


    public enum ItemType
    {
        consumable,
        throwable,
        weaponMelee,
        weaponRange,
        upgrades,
        selling,
        none,
    };

}