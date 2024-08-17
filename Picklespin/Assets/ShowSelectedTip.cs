using UnityEngine;

public class ShowSelectedTip : MonoBehaviour
{
    [SerializeField] private bool triggerOnce = true;
    private static bool triggerred = false;
    [SerializeField] private bool showAndHide = true;

    [SerializeField] private int tipIndexToDisplay;

    private TipManager tipManager;


    void Start()
    {
        tipManager = TipManager.instance;
    }


    public void Show()
    {
        if (triggerOnce && triggerred)
        {
            return;
        }

        triggerred = true;

        if (showAndHide)
        {
            tipManager.ShowAndHide(tipIndexToDisplay);
        }
        else
        {
            tipManager.Show(tipIndexToDisplay);
        }

    }
}
