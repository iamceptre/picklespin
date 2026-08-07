using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

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
    [SerializeField] private GameObject portalClosedScreen;
    private MenuScreen failScreen;

    private AudioSnapshotManager audioSnapshotManager;

    [SerializeField] private Image crosshair;

    [SerializeField][Tooltip("seconds the tint takes to swallow the screen once the fail page is up - match it to that page's entry on ScreenTint")]
    private float blackoutDuration = 2f;

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
        failScreen = MenuScreen.Of(portalClosedScreen);
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
        if (pause) pause.UnpauseGame();
        PauseGate.Block(this);

        myCollider.enabled = false;
        crosshair.enabled = false;
        playerHp.godMode = true;
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

    // the page brings the tint with it - ScreenTint darkens to whatever the fail page asks for,
    // over its own listed time, and the run only stops once that has swallowed the screen
    private IEnumerator ActivateFailScreen()
    {
        yield return new WaitForSeconds(1);

        if (failScreen) failScreen.Open();
        else DevLog.Error($"{nameof(PortalAfterClosing)}: no portal closed screen wired in - the run ends on a black screen with nothing on it", this);

        yield return new WaitForSecondsRealtime(blackoutDuration);

        if (pause) pause.PauseGamePortalClosedFail();
        else DevLog.Error($"{nameof(PortalAfterClosing)}: no {nameof(Pause)} in the scene - the clock never stops", this);
    }

}
