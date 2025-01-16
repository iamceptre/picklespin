using UnityEngine;

public class HpBarDisplay : MonoBehaviour
{
    public static HpBarDisplay Instance;
    [SerializeField] private PlayerBarDisplay playerBarDisplay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

  public void Refresh(bool smooth) { 
        playerBarDisplay.Refresh(smooth);
    }


}
