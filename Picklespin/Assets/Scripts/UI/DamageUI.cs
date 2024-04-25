using TMPro;
using UnityEngine;
using DG.Tweening;

public class DamageUI : MonoBehaviour
{

    [SerializeField] private Transform mainCamera; 
   //[HideInInspector] public Transform whoHasBeenHit;
    [HideInInspector] public Vector3 whereIshouldGo;
    public TMP_Text myText;

    private Color damageUIstartColor;
    private float damageUIstartFontSize;

    [SerializeField] private LookAtY lookAt;

    public static DamageUI instance { get; private set; } //THIS IS TEMPORARY, make is later so there can be multiple instances


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

        damageUIstartFontSize = myText.fontSize;
        damageUIstartColor = myText.color;

    }



    private void LateUpdate()
    {
        if (myText.enabled && lookAt.enabled == false) {
            lookAt.enabled = true;
        }
        else
        {
            lookAt.enabled = false;
        }
    }

   public void AnimateDamageUI()
    {
        myText.DOFade(0, 0);
        var startingYpos = myText.transform.position.y;
        var sequence = DOTween.Sequence();

        myText.DOKill();
        myText.transform.DOMoveY(whereIshouldGo.y, 0);
        myText.transform.DOScale(new Vector3(-1,1,1), 0);

        myText.transform.DOMoveY(whereIshouldGo.y + 2, 1.2f).SetEase(Ease.InSine);
        myText.transform.DOScale(new Vector3(-1.618f, 1.618f, 1.618f), 1.2f).SetEase(Ease.InSine);
        myText.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut);
    }


    private void FadeOut()
    {
        myText.DOFade(0, 1).SetEase(Ease.InSine).OnComplete(() =>
        {
            myText.enabled = false;
        });
    }


    public void WhenCritical()
    {
        myText.color = new Color(255, 215, 0);
        myText.fontSize = damageUIstartFontSize * 2;
    }

    public void WhenNotCritical()
    {
        myText.color = damageUIstartColor;
        myText.fontSize = damageUIstartFontSize;
    }

}
