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

    public void Die()
    {
        aiHealthUiBar.Detach();
        aiHealthUiBar.FadeOut();

        CheckAndDisableEffects();

        deathEvent.Invoke(); //custom death behaviour
    
        evilEntityDeathSoundReference = RuntimeManager.CreateInstance(evilEntityDeathSound);
        RuntimeManager.AttachInstanceToGameObject(evilEntityDeathSoundReference, GetComponent<Transform>());
        evilEntityDeathSoundReference.start();
        Destroy(gameObject);
    }

    private void CheckAndDisableEffects()
    {
        if (TryGetComponent<SetOnFire>(out SetOnFire setOnFire))
        {
            setOnFire.ShutFireDown();
        }
    }

}
