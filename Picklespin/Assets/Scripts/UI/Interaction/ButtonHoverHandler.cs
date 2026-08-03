using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using FMODUnity;

public class ButtonHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private TMP_Text myText;
    private Tween myTween;
    [SerializeField] private EventReference hoverSound;
    [SerializeField] bool playSoundOnHover = false;


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

        if (playSoundOnHover)
        {
            RuntimeManager.PlayOneShot(hoverSound);
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
       myTween = DOTween.To(() => myText.characterSpacing, x => myText.characterSpacing = x, 0, 0.2f).SetEase(Ease.OutExpo);
       myTween.SetUpdate(UpdateType.Normal, true);
    }

}