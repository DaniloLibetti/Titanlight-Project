
using System.Collections;
using TMPro;
using UnityEngine;

public class Item : MonoBehaviour
{
    [field: SerializeField]
    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    /*[SerializeField]
    private AudioSource audioSource;*/

    [SerializeField]
    private float duration = 0.3f;

    [SerializeField]
    private TextMeshProUGUI itemsPickedText;

    private int itemsCount = 0;

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = InventoryItem.ItemImage;
        itemsPickedText.text = "Espólios: " + itemsCount;
    }


    public void AddItem()
    {
        itemsCount++;
        //itemsPickedText.text = "Espólios: " + itemsCount;
    }


    internal void DestroyItem()
    {
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(AnimateItemPickup());
    }

    private IEnumerator AnimateItemPickup()
    {
        //audioSource.Play();
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        while(currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        Destroy(gameObject);
    }
}
