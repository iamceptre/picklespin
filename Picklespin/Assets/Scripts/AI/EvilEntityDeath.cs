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

        deathEvent.Invoke(); //custom death behaviour
    
        CheckAndDisableFire(); //if ai is burning then its dying from burninig, if theres no fire, just dies
    }


    private void CheckAndDisableFire()
    {
        var setOnFire = gameObject.GetComponentInChildren<SetOnFire>();

        if (setOnFire != null)
        {
            var setOnFireScirpt = setOnFire.GetComponent<SetOnFire>();
            //if ai is dying from fire, not spellhit
            setOnFireScirpt.PanicKill();
        }
        else
        {
            evilEntityDeathSoundReference = RuntimeManager.CreateInstance(evilEntityDeathSound);
            RuntimeManager.AttachInstanceToGameObject(evilEntityDeathSoundReference, GetComponent<Transform>());
            evilEntityDeathSoundReference.start();
            Destroy(gameObject);
        }
    }


}
