using UnityEngine;

public class Itens
{
    public enum ItemType
    {
        Potion,
        Coin,
        Scrap,
    }

    public ItemType itemType;
    public int amount;

    public Sprite GetSprite()
    {
        switch (itemType)
        {
            default:
            case ItemType.Potion: return Item_Assets.Instance.potionSprite;
            case ItemType.Scrap: return Item_Assets.Instance.scrapSprite;
            case ItemType.Coin: return Item_Assets.Instance.coinSprite;
        }
    }

}
