using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class EvilEntityDeath : MonoBehaviour
{
    [SerializeField] private EventReference evilEntityDeathSound;
    private EventInstance evilEntityDeathSoundReference;
    [SerializeField] private UnityEvent deathEvent;
    [SerializeField] private AiHealthUiBar aiHealthUiBar;

    private Dissolver dissolver;

    private void Awake()
    {
        if (aiHealthUiBar == null)
        {
            aiHealthUiBar = gameObject.GetComponent<AiHealthUiBar>();
        }
    }


    public void Die()
    {
        CheckAndDisableFire();

        if (aiHealthUiBar != null) {
            aiHealthUiBar.Detach();
            aiHealthUiBar.FadeOut();
        }

        evilEntityDeathSoundReference = RuntimeManager.CreateInstance(evilEntityDeathSound);
        RuntimeManager.AttachInstanceToGameObject(evilEntityDeathSoundReference, GetComponent<Transform>());
        evilEntityDeathSoundReference.start();
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


}
