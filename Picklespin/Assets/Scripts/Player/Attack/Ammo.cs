using UnityEngine;

public class Ammo : MonoBehaviour
{

    public int ammo;
    public int maxAmmo;

    public static Ammo instance { get; private set; }

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
