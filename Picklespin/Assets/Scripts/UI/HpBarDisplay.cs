using UnityEngine;

public class HpBarDisplay : MonoBehaviour
{
    public static HpBarDisplay instance;
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

  public void Refresh(bool smooth) { 
        playerBarDisplay.Refresh(smooth);
    }


}
