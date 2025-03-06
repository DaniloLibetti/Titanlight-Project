using UnityEngine;

public class Item_Assets : MonoBehaviour
{
    public static Item_Assets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Sprite scrapSprite;
    public Sprite coinSprite;
    public Sprite potionSprite;
}
