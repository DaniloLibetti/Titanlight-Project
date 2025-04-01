using Inventory.UI;
using UnityEngine;

public class MouseFollowe : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    [SerializeField] private UIInventoryItem item;

    public void Awake()
    {
        canvas = transform.root.GetComponent<Canvas>();
        item = GetComponentInChildren<UIInventoryItem>();
    }

    public void SetData(Sprite sprite, int quantity, ItemType itemType)
    {
        item.SetData(sprite, quantity, itemType);
    }

    private void Update()
    {
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
             Input.mousePosition, canvas.worldCamera,
             out position);
        transform.position = canvas.transform.TransformPoint(position);
    }

    public void Toggle(bool val)
    {
        Debug.Log($"Item toggled {val}");
        gameObject.SetActive( val );
    }

}
