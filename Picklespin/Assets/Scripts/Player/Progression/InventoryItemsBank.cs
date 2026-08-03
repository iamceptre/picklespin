using UnityEngine;

public class InventoryItemsBank : MonoBehaviour
{

    public static InventoryItemsBank instance;

    [Header("Picked Up Items")]
    public bool WinGateKey = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

}
