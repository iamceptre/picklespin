using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private TMP_Text myText;
    private Tween myTween;

    private void Awake()
    {
        myText = transform.GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        myText.characterSpacing = 0;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
       myTween = DOTween.To(() => myText.characterSpacing, x => myText.characterSpacing = x, 16, 0.2f).SetEase(Ease.OutExpo);
       myTween.SetUpdate(UpdateType.Normal, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       myTween = DOTween.To(() => myText.characterSpacing, x => myText.characterSpacing = x, 0, 0.2f).SetEase(Ease.OutExpo);
       myTween.SetUpdate(UpdateType.Normal, true);
    }

}