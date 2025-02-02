using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using FMODUnity;
using UnityEngine.UI;
using FMOD.Studio;

public class PortalAfterClosing : MonoBehaviour
{
    public static PortalAfterClosing instance;

    private PlayerHP playerHp;

    [SerializeField] private ParticleSystem[] particleSystems;
    private BoxCollider myCollider;
    [SerializeField] private Light portalLight;

    [SerializeField] private PostProcessVolume ppVolume;
    private ColorGrading ppColorGrading;

    private Pause pause;
    [SerializeField] private Canvas PortalClosedScreen;

    private AudioSnapshotManager audioSnapshotManager;

    [SerializeField] private CanvasGroup[] canvasToFadeout;
    [SerializeField] private Image screenTint;
    [SerializeField] private Image crosshair;

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
        audioSnapshotManager = AudioSnapshotManager.Instance;
        pause = Pause.instance;
        playerHp = PlayerHP.Instance;
        ppVolume.profile.TryGetSettings(out ppColorGrading);
    }


    public void PortalClosed()
    {
        myCollider.enabled = false;
        crosshair.enabled = false;
        playerHp.godMode = true;
        FadeOutCanvas();
        audioSnapshotManager.EnableSnapshot("Portal_Closed");
        TurnOffEmissions();
        StartCoroutine(ActivateFailScreen());
        StartCoroutine(SlowDownTimeAnDesaturate());
        portalLight.DOColor(Color.white, 1).SetEase(Ease.OutExpo);
        portalLight.DOIntensity(0, 1).OnComplete(() =>
        {
            portalLight.DOKill();
            portalLight.enabled = false;
        });
    }

    private void FadeOutCanvas()
    {
        foreach (var canvas in canvasToFadeout)
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
            ppColorGrading.saturation.value -= Time.deltaTime * 75;
            ppColorGrading.postExposure.value += Time.deltaTime * 2;
            yield return null;
        }
    }


    private void BlackOutScreen()
    {
        Tween myTween = screenTint.DOColor(Color.black, 2).OnComplete(() =>
        {
            pause.PauseGamePortalClosedFail();
        });
        myTween.SetUpdate(UpdateType.Normal, true);
    }


    private IEnumerator ActivateFailScreen()
    {
        yield return new WaitForSeconds(1);
        //audioSnapshotManager.DisableSnapshot("Portal_Closed");
        PortalClosedScreen.enabled = true;
        PortalClosedScreen.gameObject.SetActive(true); 
        myTween = failedScreenCanvasGroup.DOFade(1, 2).OnComplete(() =>
        {
            BlackOutScreen();
        });
        myTween.SetUpdate(UpdateType.Normal, true);
    }

}
