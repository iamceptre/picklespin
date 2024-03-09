using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class EvilEntityDeath : MonoBehaviour
{

    [SerializeField] private EventReference evilEntityDeathSound;
    private EventInstance evilEntityDeathSoundReference;
    public void Die()
    {
        gameObject.SetActive(false);

        evilEntityDeathSoundReference = RuntimeManager.CreateInstance(evilEntityDeathSound);
        RuntimeManager.AttachInstanceToGameObject(evilEntityDeathSoundReference, GetComponent<Transform>());
        evilEntityDeathSoundReference.start();
    }
}
