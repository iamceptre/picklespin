using UnityEngine;

public class TipManager : MonoBehaviour
{
    public static TipManager instance { private set; get; }

    [SerializeField]private TipDisplay[] tips;

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

    public void Show(int index)
    {
        
        
        tips[index].ShowTip();
    }

    public void Hide(int index)
    {
        tips[index].HideTip();
    }
}
