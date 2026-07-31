using FMODUnity;
using UnityEngine;

// Cast VFX and sound, indexed by spell ID. A spell need not appear in every array:
// a missing or short entry is skipped, so adding a spell here is optional.
public class PlayCastBlast : MonoBehaviour
{

    public static PlayCastBlast instance;
    [SerializeField] private ParticleSystem[] castBlasts;
    public ParticleSystem[] castingParticles; //longer casting
    [SerializeField] private GetParticleSizeFromCastPercentage[] castingParticleSizeScript;
    [SerializeField] private StudioEventEmitter[] castingSound;
    [SerializeField] private EventReference[] castingStartSound;

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
        if (TryGet(castBlasts, spellID, out ParticleSystem blast)) blast.Play();
    }

    public void StartCastingParticles(int spellID)
    {
        if (InRange(castingStartSound, spellID) && !castingStartSound[spellID].IsNull)
        {
            RuntimeManager.PlayOneShot(castingStartSound[spellID]);
        }
        if (TryGet(castingParticles, spellID, out ParticleSystem particles))
        {
            particles.Clear();
            particles.Play();
        }
        if (TryGet(castingSound, spellID, out StudioEventEmitter sound)) sound.Play();
        if (TryGet(castingParticleSizeScript, spellID, out GetParticleSizeFromCastPercentage sizeScript))
        {
            sizeScript.StartCoroutine(sizeScript.StartDoingShit());
        }
    }

    public void StopCastingParticles(int spellID)
    {
        if (TryGet(castingParticles, spellID, out ParticleSystem particles))
        {
            if (TryGet(castingParticleSizeScript, spellID, out GetParticleSizeFromCastPercentage sizeScript))
            {
                sizeScript.castingLight.enabled = false;
            }
            particles.Stop();
        }

        if (TryGet(castingSound, spellID, out StudioEventEmitter sound)) sound.Stop();
    }

    public void PlayCastingCompletedSound()
    {
    }

    private static bool InRange(System.Array array, int spellID) =>
        array != null && spellID >= 0 && spellID < array.Length;

    // Unity's null check, so a destroyed reference counts as "this spell has none"
    private static bool TryGet<T>(T[] array, int spellID, out T value) where T : Object
    {
        value = InRange(array, spellID) ? array[spellID] : null;
        return value != null;
    }
}
