using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using FMODUnity;

public class PortalAfterClosing : MonoBehaviour
{
    public static PortalAfterClosing instance;

    [SerializeField] private ParticleSystem[] particleSystems;
    private BoxCollider myCollider;
    [SerializeField] private Light portalLight;

    [SerializeField] private PostProcessVolume ppVolume;
    private ColorGrading ppColorGrading;

    private Pause pause;
    [SerializeField] private GameObject PortalClosedScreen;

    [SerializeField] private EventReference SnapshotAfterPortalClosage;

    [SerializeField] private CanvasGroup[] canvasToFadeOut;

    [SerializeField] private CanvasGroup failedScreenCanvasGroup;
    private Tween myTween;

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
        ppVolume.profile.TryGetSettings(out ppColorGrading);
    }


    public void PortalClosed()
    {
        myCollider.enabled = false;
        FadeOutCanvas();
        RuntimeManager.PlayOneShot(SnapshotAfterPortalClosage);
        TurnOffEmissions();
        StartCoroutine(ActivateFailScreen());
        StartCoroutine(SlowDownTimeAnDesaturate());
        //transform.DOScale(0, 1).SetEase(Ease.OutExpo);
        portalLight.DOColor(Color.white, 1).SetEase(Ease.OutExpo);
        portalLight.DOIntensity(0, 1).OnComplete(() =>
        {
            portalLight.DOKill();
            portalLight.enabled = false;
        });
    }

    private void FadeOutCanvas()
    {
        foreach (var canvas in canvasToFadeOut)
        {
            canvas.DOFade(0, 1);
        }
    }

    private void TurnOffEmissions()
    {

        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = 100;
            main.simulationSpeed = 5;
            StartCoroutine(ScaleParticles(main));
            var emission = ps.emission;
            emission.enabled = false;
        }
    }

    private IEnumerator ScaleParticles(ParticleSystem.MainModule main)
    {
        while (true)
        {
            main.startSizeMultiplier += Time.deltaTime * 100;
            yield return null;
        }
    }

    private IEnumerator SlowDownTimeAnDesaturate()
    {
        while (Time.timeScale > 0.1)
        {
            Time.timeScale -= Time.deltaTime;
            ppColorGrading.saturation.value -= Time.deltaTime * 100;
            ppColorGrading.postExposure.value += Time.deltaTime * 2;
            yield return null;
        }
    }


    private IEnumerator ActivateFailScreen()
    {
        yield return new WaitForSeconds(1);
        PortalClosedScreen.SetActive(true);
        myTween = failedScreenCanvasGroup.DOFade(1, 2).OnComplete(() =>
        {
            pause.PauseGamePortalClosedFail();
        });
        myTween.SetUpdate(UpdateType.Normal, true);
    }

}
