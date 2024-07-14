using FMODUnity;
using UnityEngine;

public class PlayCastBlast : MonoBehaviour
{

    public static PlayCastBlast instance;
    [SerializeField] private ParticleSystem[] castBlasts;
    public ParticleSystem[] castingParticles; //longer casting
    [SerializeField] private GetParticleSizeFromCastPercentage[] castingParticleSizeScript;
    [SerializeField] private StudioEventEmitter[] castingSound;
    [SerializeField] private EventReference[] castingStartSound;

    private int spellIDcached;

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
    }


    public void Play(int spellID)
    {
        castBlasts[spellID].Play();
        spellIDcached = spellID;
    }

    public void StartCastingParticles(int spellID)
    {
        RuntimeManager.PlayOneShot(castingStartSound[spellID]);
        spellIDcached = spellID;
        castingParticles[spellID].Clear();
        castingParticles[spellID].Play();
        castingSound[spellID].Play();
        castingParticleSizeScript[spellID].StartCoroutine(castingParticleSizeScript[spellID].StartDoingShit());
    }

    public void StopCastingParticles(int spellID)
    {
        if (castingParticles[spellID] != null)
        {
            castingParticles[spellID].Stop();
        }

        if (castingSound != null)
        {
            castingSound[spellID].Stop();
        }
    }

    public void PlayCastingCompletedSound()
    {
        //castingSound[spellIDcached].Play();
    }
}
