using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class EvilEntityDeath : MonoBehaviour
{
    [SerializeField] private UnityEvent deathEvent;
    [SerializeField] private AiHealthUiBar aiHealthUiBar;

    private Dissolver dissolver;

    private CameraShakeManagerV2 camShakeManager;
    private ScreenFlashTint screenFlashTint;

    private void Awake()
    {
        if (aiHealthUiBar == null)
        {
            aiHealthUiBar = gameObject.GetComponent<AiHealthUiBar>();
        }
    }

    private void Start()
    {
        camShakeManager = CameraShakeManagerV2.instance;
        screenFlashTint = ScreenFlashTint.instance;
    }


    public void Die()
    {
        CheckAndDisableFire();

        if (aiHealthUiBar != null) {
            aiHealthUiBar.Detach();
            aiHealthUiBar.FadeOut();
        }

        screenFlashTint.Flash(6);
        StartCoroutine(ShakeLater());
        dissolver = gameObject.GetComponent<Dissolver>();
        dissolver.StartDissolve();

        deathEvent.Invoke(); //additional death behaviour

    }


    private void CheckAndDisableFire()
    {
        if (gameObject.TryGetComponent<SetOnFire>(out SetOnFire setOnFireScirpt))
        {
            setOnFireScirpt.KillFromFire(); //die during being on fire
        }
    }


    private IEnumerator ShakeLater()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        camShakeManager.ShakeSelected(6);
    }


}
