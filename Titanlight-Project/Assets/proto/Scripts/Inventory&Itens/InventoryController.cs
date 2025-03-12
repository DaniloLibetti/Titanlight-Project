using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private InventoryUi inventoryUi;

    public int inventorySize = 30;

    private void Start()
    {
        inventoryUi.InitializeInventoryUI(inventorySize);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if(inventoryUi.isActiveAndEnabled == false)
            {
                inventoryUi.Show();
            }
            else
            {
                inventoryUi.Hide();
            }
        }
    }
}
