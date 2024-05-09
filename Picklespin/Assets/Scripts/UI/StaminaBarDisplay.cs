using UnityEngine;
public class StaminaBarDisplay : MonoBehaviour
{
    public static StaminaBarDisplay instance { get; private set; }
    [SerializeField] private PlayerBarDisplay playerBarDisplay;


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
    private void Start()
    {
        Refresh(false);
    }

    public void Refresh(bool smooth)
    {
        playerBarDisplay.Refresh(smooth);
    }

}
