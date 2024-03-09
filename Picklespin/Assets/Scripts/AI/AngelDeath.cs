using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AngelDeath : MonoBehaviour
{

    [SerializeField] private EventReference angelDeathSound;
    private EventInstance angelDeathInstance;
    public void Die()
    {
        gameObject.SetActive(false);

        angelDeathInstance = RuntimeManager.CreateInstance(angelDeathSound);
        RuntimeManager.AttachInstanceToGameObject(angelDeathInstance, GetComponent<Transform>());
        angelDeathInstance.start();
    }
}
