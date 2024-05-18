using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class EvilEntityDeath : MonoBehaviour
{
    ManaSuckParticlesSpawner manaSuckParticlesSpawner;
    [SerializeField] private EventReference evilEntityDeathSound;
    private EventInstance evilEntityDeathSoundReference;
    [SerializeField] private UnityEvent deathEvent;
    [SerializeField] private AiHealthUiBar aiHealthUiBar;

    private Dissolver dissolver;

    [SerializeField] private int howMuchManaIGiveAfterDying = 25;
    private Ammo ammo;

    private void Awake()
    {
        if (aiHealthUiBar == null)
        {
            aiHealthUiBar = gameObject.GetComponent<AiHealthUiBar>();
        }
    }

    private void Start()
    {
        manaSuckParticlesSpawner = ManaSuckParticlesSpawner.instance;
        ammo = Ammo.instance;
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

        ammo.GiveManaToPlayer(howMuchManaIGiveAfterDying);
        manaSuckParticlesSpawner.Spawn(transform.position, howMuchManaIGiveAfterDying);

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
