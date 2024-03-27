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
        myLight.color = new Color(0, 0, 0);
        FadeIn();

        if (myTransform != null)
        {
            myTransform.DOShakePosition(lightDuration, 0.1f, 5, 90, false, false, ShakeRandomnessMode.Harmonic);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            FadeOut();
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

    void FadeOut()
    {
        if (myLight != null)
        {
            myLight.DOColor(new Color(0, 0, 0), 1).OnComplete(Die);
        }
    }

   public void Die()
    {
        myTransform.DOKill();
        myLight.DOKill();
        Destroy(gameObject);
    }

}
