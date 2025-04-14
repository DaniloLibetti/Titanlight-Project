using Inventory.UI;
using UnityEngine;
using UnityEngine.UI;


namespace Inventory.UI
{
    public class EquipmentSlot : MonoBehaviour
    {
        [SerializeField] private Image slotImage;

        //[SerializeField] private ItemType itemType = new ItemType();

        private Sprite itemSprite;

        //private bool slotInUse = false;

        public void EquipGear(Sprite itemImage)
        {
            this.itemSprite = itemImage;
            slotImage.sprite = this.itemSprite;

            //slotInUse = true;
        }
    }
}

