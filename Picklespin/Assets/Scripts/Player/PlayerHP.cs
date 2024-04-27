using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public static PlayerHP instance { get; private set; }

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

    public int hp;
    public int maxHp;

}
