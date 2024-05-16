using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
public class AddedEXP_Gui : MonoBehaviour
{
    public static AddedEXP_Gui instance { get; private set; }
    private TMP_Text _text;
    private int amountToShow = 0;

    private Vector2 _textStartPosition;

    private RectTransform _textTransform;

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
        _text.enabled = false;
    }

    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            DisplayAddedEXP(100, "debug");
        }

    }
    */

    public void DisplayAddedEXP(int addedXP, string expSourceName)
    {
        _text.enabled = true;
        amountToShow = amountToShow + addedXP;
        _text.text = "+ " + amountToShow + " exp" + "<size=15px>" + " (" + expSourceName + ")";
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

    private IEnumerator WaitAndFadeOut()
    {
        yield return new WaitForSeconds(1);
        _textTransform.DOLocalMoveY(_textStartPosition.y + 20, 2).SetEase(Ease.InSine);
        _text.DOFade(0, 2).SetEase(Ease.InSine).OnComplete(() =>
        {
            amountToShow = 0;
            _text.enabled = false;
        });
    }

}
