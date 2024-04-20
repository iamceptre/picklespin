using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class EvilEntityDeath : MonoBehaviour
{
    [SerializeField] private EventReference evilEntityDeathSound;
    private EventInstance evilEntityDeathSoundReference;
    [SerializeField] private GameObject aiUi;
    [SerializeField] private UnityEvent deathEvent;
    public void Die()
    {
        deathEvent.Invoke();
        aiUi.transform.SetParent(null);
        aiUi.transform.position += new Vector3(0,0.8f);
        gameObject.SetActive(false);
        evilEntityDeathSoundReference = RuntimeManager.CreateInstance(evilEntityDeathSound);
        RuntimeManager.AttachInstanceToGameObject(evilEntityDeathSoundReference, GetComponent<Transform>());
        evilEntityDeathSoundReference.start();
    }
}
