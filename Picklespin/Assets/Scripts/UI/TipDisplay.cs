using DG.Tweening;
using TMPro;
using UnityEngine;

public class TipDisplay : MonoBehaviour
{

    private TMP_Text myText;

    private void Awake()
    {
        myText = GetComponent<TMP_Text>();
    }


    public void ShowTip()
    {
        myText.enabled = true;
        myText.DOKill();
        myText.DOFade(1, 0.2f);
    }


    public void HideTip()
    {
         myText.DOFade(0, 0.63f).OnComplete(DisableMe);
    }

    private void DisableMe()
    {
        myText.enabled = false;
        gameObject.SetActive(false);    
    }

}
