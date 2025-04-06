using Inventory.UI;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour
{
    [SerializeField] private Image slotImage;

    [SerializeField] private ItemType itemType = new ItemType();

    private Sprite itemSprite;

    private bool slotInUse;

    public void EquipGear(int itemIndex, Sprite itemImage)
    {
        this.itemSprite = itemImage;
        slotImage.sprite = this.itemSprite;

        slotInUse = true;
    }
}
