using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;
using System.Collections;

public class EvilEntityDeath : MonoBehaviour
{
    //[SerializeField] private EventReference evilEntityDeathSound;
    //private EventInstance evilEntityDeathSoundReference;
    [SerializeField] private UnityEvent deathEvent;
    [SerializeField] private AiHealthUiBar aiHealthUiBar;

    private Dissolver dissolver;

    private CameraShakeManagerV2 camShakeManager;

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
    }


    public void Die()
    {
        CheckAndDisableFire();

        if (aiHealthUiBar != null) {
            aiHealthUiBar.Detach();
            aiHealthUiBar.FadeOut();
        }

        //evilEntityDeathSoundReference = RuntimeManager.CreateInstance(evilEntityDeathSound);
        //RuntimeManager.AttachInstanceToGameObject(evilEntityDeathSoundReference, GetComponent<Transform>());
        //evilEntityDeathSoundReference.start();
        //evilEntityDeathSoundReference.release();
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
