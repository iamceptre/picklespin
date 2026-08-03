using UnityEngine;

public class AmmoDisplay : MonoBehaviour
{
    public static AmmoDisplay instance { get; private set; }
    [SerializeField] private PlayerBarDisplay playerBarDisplay;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void Refresh(bool smooth)
    {
        playerBarDisplay.Refresh(smooth);
    }

    public void SetContinuousValue(float value, float maxValue)
    {
        playerBarDisplay.SetContinuousValue(value, maxValue);
    }
}
