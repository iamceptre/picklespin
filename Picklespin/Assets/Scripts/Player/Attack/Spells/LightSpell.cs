using UnityEngine;
using DG.Tweening;
using System.Collections;

public class LightSpell : MonoBehaviour, ISpellBehaviour
{

    [SerializeField] private Light myLight;
    [SerializeField] private Transform myTransform;

    [SerializeField] private float lightDuration;

    private WaitForSeconds timeBeforeOut;
    [SerializeField] private Bullet bullet;

    private bool convertedThisFlight;

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

    private void OnDisable()
    {
        StopAllCoroutines();
        if (gameObject.IsUnloading()) return;

        myLight.DOKill();
        myTransform.DOKill();
        myTransform.localPosition = Vector3.zero;
    }

    private void FadeIn()
    {
        myLight.DOColor(Color.white, 0.5f).OnComplete(RunRoutine);
    }

    private void RunRoutine()
    {
        if (!isActiveAndEnabled) return;
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
        if (!isActiveAndEnabled) return;

        StopAllCoroutines();

        myLight.DOKill();
        myLight.DOColor(Color.black, 1).OnComplete(Die);
    }

    public bool InterceptHit(AiReferences refs, bool keepFlying)
    {
        if (!keepFlying) FadeOut();
        if (!PlayerClasses.LightSpellConverts) return false;

        convertedThisFlight = true;
        ConvertedAlly.Convert(refs);
        return true;
    }

    public void OnImpact(Vector3 point)
    {
        if (PlayerClasses.LightSpellConverts && !convertedThisFlight) ConvertedAlly.CommandAll(point);
    }

    public void ResetForFlight() => convertedThisFlight = false;

    public bool TryRetire()
    {
        if (!isActiveAndEnabled) return false;

        FadeOut();
        return true;
    }

   private void Die()
    {
        myTransform.DOKill();
        myLight.DOKill();
        bullet.ReturnToPool();
    }

}
