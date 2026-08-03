using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;
using System.Text;

public class DamageUIV2 : MonoBehaviour
{
    private TMP_Text myText;
    private Transform myText_Transform;
    private ObjectPool<DamageUIV2> pool;

    private Color damageUIstartColor;
    private float damageUIstartFontSize;

    private static readonly Color criticalColor = GameColors.Critical;

    private readonly StringBuilder sb = new StringBuilder();

    private void Awake()
    {
        myText = GetComponent<TMP_Text>();
        myText_Transform = transform;
        // captured once: reuse must not re-capture crit-scaled values
        damageUIstartFontSize = myText.fontSize;
        damageUIstartColor = myText.color;
    }

    public void SetPool(ObjectPool<DamageUIV2> pool)
    {
        this.pool = pool;
    }

    public void Do(Vector3 whereIshouldGo, int howMuchDamageDealt, bool isCritical)
    {
        myText_Transform.DOKill();
        myText.DOKill();
        myText_Transform.position = whereIshouldGo;
        myText_Transform.localScale = new Vector3(-1, 1, 1);
        myText.color = damageUIstartColor.WithAlpha(0f);

        sb.Clear();
        sb.Append(isCritical ? "- <b>" : "- ");
        sb.Append(howMuchDamageDealt);
        myText.text = sb.ToString();

        if (isCritical)
        {
            myText.color = criticalColor;
            myText.fontSize = damageUIstartFontSize * 1.6f;
        }
        else
        {
            FadeInFlashColor();
            myText.fontSize = damageUIstartFontSize;
        }

        myText_Transform.DOMoveY(whereIshouldGo.y + 2, 1.2f).SetEase(Ease.InSine);
        myText_Transform.DOScale(new Vector3(-1.6f, 1.6f, 1.6f), 1.2f).SetEase(Ease.InSine);
        myText.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut);
    }

    private void FadeInFlashColor()
    {
        myText.color = GameColors.Neutral;
        myText.DOColor(damageUIstartColor, 0.15f);
    }

    private void FadeOut()
    {
        myText.DOFade(0, 1).SetEase(Ease.InSine).OnComplete(() =>
        {
            myText_Transform.DOKill();
            myText.DOKill();
            if (pool != null) pool.Release(this);
            else Destroy(gameObject);
        });
    }
}
