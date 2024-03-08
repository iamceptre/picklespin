using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;

public class SelectedSpellDisplay : MonoBehaviour
{

    [SerializeField] private Attack attackScript;
    private TMP_Text selectedSpellText;

    private void Awake()
    {
        selectedSpellText = GetComponent<TMP_Text>();
        selectedSpellText.text = attackScript.bulletPrefab[attackScript.selectedBullet].gameObject.name.ToString();
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

        var sequence = DOTween.Sequence();

        selectedSpellText.DOKill();
        StopAllCoroutines();
        selectedSpellText.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut); //fade IN
    }


    private void FadeOut()
    {
        selectedSpellText.DOFade(0, 1.2944f).SetEase(Ease.InSine).OnComplete(DisableMe);
    }

}
