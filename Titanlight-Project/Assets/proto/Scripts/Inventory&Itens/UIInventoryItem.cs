using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Inventory.Model;
using Unity.VisualScripting;

namespace Inventory.UI
{
    public class UIInventoryItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text quantityTxt;
        [SerializeField] private Image borderImage;

        private Sprite itemSprite;

        public event Action<UIInventoryItem> OnItemClicked, OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag, OnRightMouseButtonClick;

        private int itemPrice;
        private int itemIndex;
        

        private bool empty = true;
        //public bool isItemSelected = false;

        public ItemType itemType;

        //[SerializeField]private EquipmentSlot meleeSlot, rangeSlot, upgradeSlot1, upgradeSlot2, throwableSlot, consumableSlot;

        private void Awake()
        {
            ResetData();
            Deselect();
        }

        //Reseta os dados
        public void ResetData()
        {
            itemImage.gameObject.SetActive(false);
            empty = true;
        }

        //Desseleciona o item no inventario
        public void Deselect()
        {
            borderImage.enabled = false;
        }

        //Define os dados do item
        public void SetData(Sprite sprite, int quantity, ItemType itemType)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = sprite;
            quantityTxt.text = quantity + "";
            this.itemType = itemType;
            empty = false;
        }

        //Seleciona o item no inventario
        public void Select()
        {
            borderImage.enabled = true;
        }

        public void OnPointerClick(PointerEventData pointerData)
        {

            if (/*isItemSelected = true &*/ pointerData.button == PointerEventData.InputButton.Right)
            {
                //EquipGear();
                OnRightMouseButtonClick?.Invoke(this);
            }
            else
            {
                OnItemClicked?.Invoke(this);
            }
        }

        /*public void EquipGear()
        {

            if (itemType == ItemType.weaponMelee)
                meleeSlot.EquipGear(itemSprite);
            if (itemType == ItemType.weaponRange)
                rangeSlot.EquipGear(itemSprite);
            if (itemType == ItemType.upgrades)
                upgradeSlot1.EquipGear(itemSprite);
            if (itemType == ItemType.upgrades)
                upgradeSlot2.EquipGear(itemSprite);
            if (itemType == ItemType.throwable)
                throwableSlot.EquipGear(itemSprite);
            if (itemType == ItemType.consumable)
                consumableSlot.EquipGear(itemSprite);


        }*/

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (empty)
            {
                return;
            }
            OnItemBeginDrag?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnItemEndDrag?.Invoke(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            OnItemDroppedOn?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {

        }
    }
}