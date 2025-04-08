using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private InventoryUi inventoryUi;

        //[SerializeField] private InventorySO inventoryData;

        [SerializeField] private InventoryData inventoryData;

        public List<InventoryItem> initialItems = new List<InventoryItem>();


        private void Start()
        {
            PrepareUI();
            PrepareInventoryData();
        }

        private void PrepareInventoryData()
        {
            //inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;
            foreach (InventoryItem item in initialItems)
            {
                if (item.isEmpty)
                {
                    continue;
                }
                inventoryData.AddItem(item);
            }
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
        {
            inventoryUi.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUi.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity);
            }
        }

        private void PrepareUI()
        {
            //inventoryUi.InitializeInventoryUI(inventoryData.Size);
            //this.inventoryUi.OnSwapItems += HandleSwapItems;
            //this.inventoryUi.OnStartDragging += HandleDragging;
            this.inventoryUi.OnItemSelection += HandleItemSelection;
        }

        private void HandleItemSelection(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.isEmpty)
            {
                inventoryUi.ResetSelection();
                //inventoryUi.isItemSelected = false;
                return;
            }
            ItemSO item = inventoryItem.item;
        }

        /*public void PerformAction(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.isEmpty)
            {
                return;
            }
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryData.RemoveItem(itemIndex, 1);
            }
            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                itemAction.PerformAction(gameObject, inventoryItem.itemState);
            }
        }*/

        /*private void HandleDragging(int itemIndex)
        {
            InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.isEmpty)
            {
                return;
            }
            inventoryUi.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity, inventoryItem.itemType);
        }*/

        /*private void HandleSwapItems(int itemIndex1, int itemIndex2)
        {
            inventoryData.SwapItems(itemIndex1, itemIndex2);
        }*/

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (inventoryUi.isActiveAndEnabled == false)
                {
                    inventoryUi.Show();
                    foreach (var item in inventoryData.GetCurrentInventoryState())
                    {
                        inventoryUi.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity);
                    }
                }
                else
                {
                    inventoryUi.Hide();
                }
            }
        }
    }
}