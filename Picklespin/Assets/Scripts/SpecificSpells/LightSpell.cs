using UnityEngine;
using DG.Tweening;
using System.Collections;

public class LightSpell : MonoBehaviour
{

    [SerializeField] private Light myLight;
    [SerializeField] private Transform myTransform;

    [SerializeField] private float lightDuration;

    void Start()
    {
        myLight.DOKill();
        myLight.color = new Color(0, 0, 0);
        FadeIn();

        if (myTransform != null)
        {
            myTransform.DOShakePosition(lightDuration, 0.1f, 5, 90, false, false, ShakeRandomnessMode.Harmonic);
        }
    }

    private void FadeIn()
    {
        myLight.DOColor(new Color(1, 1, 1), 0.5f).OnComplete(RunRoutine);
    }

    private void RunRoutine()
    {
        StartCoroutine(WaitAndFadeOut());
    }

    private IEnumerator WaitAndFadeOut()
    {
        yield return new WaitForSeconds(lightDuration);
        FadeOut();
        yield return null;
    }

    public void FadeOut()
    {
        if (myLight != null)
        {
            myLight.DOColor(new Color(0, 0, 0), 1).OnComplete(Die);
        }
        else
        {
            myLight.DOKill();
        }
    }

   private void Die()
    {
        myTransform.DOKill();
        myLight.DOKill();
        Destroy(gameObject);
    }

}
