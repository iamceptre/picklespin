using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] private Slider manaBar;
    [SerializeField] private PlayerBarDisplay playerBarDisplay;

    public static AmmoDisplay instance { get; private set; }


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

    public void Refresh(bool smooth)
    {
        playerBarDisplay.Refresh(smooth);
    }

}
