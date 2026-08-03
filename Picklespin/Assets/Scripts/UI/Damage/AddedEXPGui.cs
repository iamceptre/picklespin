using DG.Tweening;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class AddedEXPGui : MonoBehaviour
{
    private Canvas myCanvas;
    public static AddedEXPGui instance { get; private set; }
    private TMP_Text _text;
    private int amountToShow = 0;
    private Vector2 _textStartPosition;
    private RectTransform _textTransform;
    private Color startingColor;
    private WaitForSeconds waitBeforeFadingTime = new WaitForSeconds(3);
    private StringBuilder sb = new StringBuilder();
    private Color negativeColor = GameColors.Health;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
        _text = GetComponent<TMP_Text>();
        _textTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        _textStartPosition = transform.localPosition;
        startingColor = _text.color.WithAlpha(1f);
        myCanvas = GetComponent<Canvas>();
        myCanvas.enabled = false;
    }

    public void DisplayAddedEXP(int addedXP, string expSourceName)
    {
        myCanvas.enabled = true;
        amountToShow += addedXP;
        UpdateText(addedXP, expSourceName);
        _text.DOKill();
        _textTransform.DOKill();
        _textTransform.localPosition = _textStartPosition;
        _text.DOFade(1, 0.4f);
        _textTransform.DOScale(1, 0.05f).OnComplete(() =>
        {
            _textTransform.DOScale(1.6f, 0.2f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                _textTransform.DOScale(1f, 0.324f);
            });
        });
        StopAllCoroutines();
        StartCoroutine(WaitAndFadeOut());
    }

    private void UpdateText(int addedXP, string expSourceName)
    {
        sb.Clear();
        if (addedXP < 0) _text.color = negativeColor;
        else _text.color = startingColor;
        sb.Append(amountToShow.ToString("+#;-#;0"));
        sb.Append(" Exp<size=15px><br>");
        sb.Append(expSourceName);
        _text.text = sb.ToString();
    }

    private IEnumerator WaitAndFadeOut()
    {
        yield return waitBeforeFadingTime;
        _textTransform.DOLocalMoveY(_textStartPosition.y + 20, 2).SetEase(Ease.InSine);
        _text.DOFade(0, 2).SetEase(Ease.InSine).OnComplete(() =>
        {
            amountToShow = 0;
            myCanvas.enabled = false;
        });
    }
}
