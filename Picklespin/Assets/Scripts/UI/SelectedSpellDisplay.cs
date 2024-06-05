using TMPro;
using UnityEngine;
using DG.Tweening;

public class SelectedSpellDisplay : MonoBehaviour
{

     private Attack attackScript;
     private TMP_Text selectedSpellText;

    private void Start()
    {
        selectedSpellText = GetComponent<TMP_Text>();
        attackScript = Attack.instance;
        selectedSpellText.text = attackScript.currentBullet.spellName;
        selectedSpellText.color = new Color(255, 255, 255, 0);
        UpdateText();
        selectedSpellText.DOKill();
        selectedSpellText.DOFade(0, 0);
    }

    private void DisableMe()
    {
        gameObject.SetActive(false);
    }

    public void UpdateText()
    {
        selectedSpellText.text = attackScript.currentBullet.spellName;
        Animate();
    }


    private void Animate()
    {
        selectedSpellText.DOKill();
        StopAllCoroutines();
        selectedSpellText.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut);
    }

    private void FadeOut()
    {
        selectedSpellText.DOFade(0, 1.37f).SetEase(Ease.InSine).OnComplete(DisableMe);
    }


}
