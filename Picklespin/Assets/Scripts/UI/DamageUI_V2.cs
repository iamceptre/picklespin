using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Text;

public class DamageUI_V2 : MonoBehaviour
{
    private TMP_Text myText;
    private Transform myText_Transform;

    private Color damageUIstartColor;
    private float damageUIstartFontSize;



    private Color criticalColor = new Color(255, 215, 0);


    private void Awake()
    {
        myText = GetComponent<TMP_Text>();
        myText_Transform = gameObject.transform;
    }

    private void OnEnable()
    {
        damageUIstartFontSize = myText.fontSize;
        damageUIstartColor = myText.color;
    }


    public void Do(Vector3 whereIshouldGo, int howMuchDamageDealt, bool isCritical)
    {
        myText_Transform.position = whereIshouldGo;
        myText_Transform.localScale = new Vector3(-1, 1, 1);

        myText.color = new Color(myText.color.r, myText.color.g, myText.color.b, 0);

        StringBuilder sb = new StringBuilder();

        if (isCritical)
        {
            sb.Append("- <b>");
            sb.Append(howMuchDamageDealt);
            myText.text = sb.ToString();
            myText.color = criticalColor;
            myText.fontSize = damageUIstartFontSize * 1.618f;
        }
        else
        {
            sb.Append("- ");
            sb.Append(howMuchDamageDealt);
            myText.text = sb.ToString();
            FadeInFlashColor();
            //myText.color = damageUIstartColor;
            myText.fontSize = damageUIstartFontSize;
        }

        myText_Transform.DOMoveY(whereIshouldGo.y + 2, 1.2f).SetEase(Ease.InSine);
        myText_Transform.DOScale(new Vector3(-1.62f, 1.62f, 1.62f), 1.2f).SetEase(Ease.InSine);
        myText.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut);
    }


    private void FadeInFlashColor()
    {
        myText.color = Color.white;
        myText.DOKill();
        myText.DOColor(damageUIstartColor, 0.15f);
    }


    private void FadeOut()
    {
        myText.DOFade(0, 1).SetEase(Ease.InSine).OnComplete(() =>
        {
            myText_Transform.DOKill();
            myText.DOKill();
            Destroy(gameObject);
        });
    }


}
