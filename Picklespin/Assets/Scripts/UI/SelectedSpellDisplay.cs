using TMPro;
using UnityEngine;
using DG.Tweening;

public class SelectedSpellDisplay : MonoBehaviour
{

    [SerializeField] private Attack attackScript;
    private TMP_Text selectedSpellText;

    private void Awake()
    {
        selectedSpellText = GetComponent<TMP_Text>();
        selectedSpellText.text = attackScript.bulletPrefab[attackScript.selectedBullet].gameObject.name.ToString();
        selectedSpellText.color = new Color(255,255,255,0);
        UpdateText();
    }

private void DisableMe()
    {
        gameObject.SetActive(false); 
    }

 public void UpdateText()
    {
        if (selectedSpellText != null)
        {
            selectedSpellText.text = attackScript.bulletPrefab[attackScript.selectedBullet].gameObject.name.ToString();
        }
        Animate();
    }


    private void Animate()
    {
        //var sequence = DOTween.Sequence();
        selectedSpellText.DOKill();
        StopAllCoroutines();
        selectedSpellText.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut);
    }

    private void FadeOut()
    {
        selectedSpellText.DOFade(0, 1.37f).SetEase(Ease.InSine).OnComplete(DisableMe);
    }


}
