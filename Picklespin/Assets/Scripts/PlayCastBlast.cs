using UnityEngine;

public class PlayCastBlast : MonoBehaviour
{

    public static PlayCastBlast instance;
    [SerializeField] private ParticleSystem[] castBlasts;
    public ParticleSystem[] castingParticles; //longer casting
    [SerializeField] private GetParticleSizeFromCastPercentage[] castingParticleSizeScript;

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
    }

    public void StartCastingParticles(int spellID)
    {
        castingParticles[spellID].Play();
        castingParticleSizeScript[spellID].StartCoroutine(castingParticleSizeScript[spellID].StartDoingShit());
    }

    public void StopCastingParticles(int spellID)
    {
        castingParticles[spellID].Stop();
    }
}
