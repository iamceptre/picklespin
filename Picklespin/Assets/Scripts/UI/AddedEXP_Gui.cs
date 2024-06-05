using DG.Tweening;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
public class AddedEXP_Gui : MonoBehaviour
{
    private Canvas myCanvas;
    public static AddedEXP_Gui instance { get; private set; }
    private TMP_Text _text;
    private int amountToShow = 0;

    private Vector2 _textStartPosition;

    private RectTransform _textTransform;
    private Color startingColor;

    private WaitForSeconds waitBeforeFadingTime = new WaitForSeconds(1);

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

        _text = GetComponent<TMP_Text>();
        _textTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        _textStartPosition = transform.localPosition;
        startingColor = new Color(_text.color.r, _text.color.g, _text.color.b, 1);
        myCanvas = gameObject.GetComponent<Canvas>();
        myCanvas.enabled = false;
    }

    public void DisplayAddedEXP(int addedXP, string expSourceName)
    {
        myCanvas.enabled = true;
        amountToShow = amountToShow + addedXP;
        UpdateText(addedXP, expSourceName);
        _text.DOKill();
        _textTransform.DOKill();
        _textTransform.localPosition = _textStartPosition;
        _text.DOFade(1, 0.4f);

        _textTransform.DOScale(1, 0.05f).OnComplete(() =>
        {
            _textTransform.DOScale(1.618f, 0.2f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                _textTransform.DOScale(1f, 0.324f);
            });
        });

        StopAllCoroutines();
        StartCoroutine(WaitAndFadeOut());
    }

    private void UpdateText(int addedXP, string expSourceName)
    {
        if (addedXP > 0)
        {
            _text.color = startingColor;
            _text.text =  "+ " + amountToShow + " exp<size=15px><br>(" + expSourceName + ")";
        }
        else
        {
            _text.color = new Color(0.76f, 0.235f, 0.235f);
            _text.text = amountToShow + " exp<size=15px><br>(" + expSourceName + ")";
        }
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
