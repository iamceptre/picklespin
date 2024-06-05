using UnityEngine;
using DG.Tweening;
using System.Collections;

public class LightSpell : MonoBehaviour
{

    [SerializeField] private Light myLight;
    [SerializeField] private Transform myTransform;

    [SerializeField] private float lightDuration;

    private WaitForSeconds timeBeforeOut;
    [SerializeField] private Bullet bullet;


    void Awake()
    {
        timeBeforeOut = new WaitForSeconds(lightDuration);
        myLight.color = Color.black;
    }

    private void OnEnable()
    {
        myLight.color = Color.black;
        myLight.DOKill();
        FadeIn();

        myTransform.DOKill();
        myTransform.localPosition = Vector3.zero;
        myTransform.DOShakePosition(lightDuration, 0.1f, 5, 90, false, false, ShakeRandomnessMode.Harmonic);
    }


    private void FadeIn()
    {
        myLight.DOColor(Color.white, 0.5f).OnComplete(RunRoutine);
    }

    private void RunRoutine()
    {
        StartCoroutine(WaitAndFadeOut());
    }

    private IEnumerator WaitAndFadeOut()
    {
        yield return timeBeforeOut;
        FadeOut();
        yield break;
    }

    public void FadeOut()
    {
            StopAllCoroutines();
            myLight.DOColor(Color.black, 1).OnComplete(Die);
    }

   private void Die()
    {
        myTransform.DOKill();
        myLight.DOKill();
        bullet.ReturnToPool();
    }

}
