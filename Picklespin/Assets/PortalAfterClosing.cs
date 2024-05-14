using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;

public class PortalAfterClosing : MonoBehaviour
{
    public static PortalAfterClosing instance;

    [SerializeField] private ParticleSystem[] particleSystems;
    private BoxCollider myCollider;
    private Death death;
    [SerializeField] private Light portalLight;

    [SerializeField] private PostProcessVolume ppVolume;
    private ColorGrading ppColorGrading;

    private Pause pause;
    [SerializeField] private GameObject PortalClosedScreen;

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
        myCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        pause = Pause.instance;
        death = Death.instance;
        ppVolume.profile.TryGetSettings(out ppColorGrading);
    }


    public void PortalClosed()
    {
        myCollider.enabled = false;
        TurnOffEmissions();
        StartCoroutine(ActivateFailScreen());
        StartCoroutine(SlowDownTimeAnDesaturate());
        transform.DOScale(0, 1).SetEase(Ease.OutExpo);
        portalLight.DOColor(Color.white, 1).SetEase(Ease.OutExpo);
        portalLight.DOIntensity(0, 1).OnComplete(() =>
        {
            portalLight.DOKill();
            portalLight.enabled = false;
        });
    }

    private void TurnOffEmissions()
    {

        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 15;
            var emission = ps.emission;
            emission.enabled = false;
        }
    }

    private IEnumerator SlowDownTimeAnDesaturate()
    {
        while (Time.timeScale > 0.1) {
            Time.timeScale -= Time.deltaTime;
            ppColorGrading.saturation.value -= Time.deltaTime * 50;
            ppColorGrading.postExposure.value += Time.deltaTime;
            yield return null;
        }
    }


    private IEnumerator ActivateFailScreen()
    {
        yield return new WaitForSeconds(1);
        PortalClosedScreen.SetActive(true);
        pause.PauseGamePortalClosedFail();
    }

}
