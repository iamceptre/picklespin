using UnityEngine;
using TMPro;
using System.Collections;

public class TipManager : MonoBehaviour
{
    public static TipManager instance { private set; get; }

    [SerializeField] private TipDisplay[] tips;

    private WaitForSeconds waitBeforeHidingTip = new WaitForSeconds(2);

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


    public void ShowAndHide(int index)
    {
        tips[index].ShowTip();
        StartCoroutine(ShowAndHider(index));
    }

    public bool AreAnyTipsActive()
    {
        for (int i = 0; i < tips.Length; i++)
        {
            if (tips[i].GetComponent<TMP_Text>().enabled)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator ShowAndHider(int index)
    {
        yield return waitBeforeHidingTip;
        tips[index].HideTip();
    }
}