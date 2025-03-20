using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    private Going currentPortal;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && currentPortal != null && currentPortal.CanTeleport())
        {
            transform.position = currentPortal.GetOtherPoint(transform.position);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Going>())
        {
            currentPortal = other.GetComponent<Going>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Going>())
        {
            currentPortal = null;
        }
    }
}


