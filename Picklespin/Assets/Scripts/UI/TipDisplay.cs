using DG.Tweening;
using TMPro;
using UnityEngine;

public class TipDisplay : MonoBehaviour
{

    private TMP_Text myText;
    private Canvas _canvas;
    private TipManager tipManager;

    [SerializeField] private float fadeInTime = 0.2f;

    private void Awake()
    {
        myText = GetComponent<TMP_Text>();
        myText.enabled = false;
        _canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        tipManager = TipManager.instance;
    }

    public void ShowTip()
    {
        _canvas.enabled = true;
        myText.enabled = true;
        myText.DOKill();
        myText.DOFade(1, fadeInTime);
    }


    public void HideTip()
    {
        myText.DOFade(0, fadeInTime * 1.62f).OnComplete(() =>
        {
            myText.enabled = false;

            var areAnyActive = tipManager.AreAnyTipsActive();
            if (!areAnyActive)
            {
                _canvas.enabled = false;
            }
        });
    }

}
