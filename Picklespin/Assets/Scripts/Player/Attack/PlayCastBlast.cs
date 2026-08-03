using FMODUnity;
using UnityEngine;

// Cast VFX and sound, indexed by SpellId. A spell need not appear in every array:
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

    public void Play(SpellId spell)
    {
        if (TryGet(castBlasts, (int)spell, out ParticleSystem blast)) blast.Play();
    }

    public void StartCastingParticles(SpellId spell)
    {
        int slot = (int)spell;

        if (InRange(castingStartSound, slot) && !castingStartSound[slot].IsNull)
        {
            RuntimeManager.PlayOneShot(castingStartSound[slot]);
        }
        if (TryGet(castingParticles, slot, out ParticleSystem particles))
        {
            particles.Clear();
            particles.Play();
        }
        if (TryGet(castingSound, slot, out StudioEventEmitter sound)) sound.Play();
        if (TryGet(castingParticleSizeScript, slot, out GetParticleSizeFromCastPercentage sizeScript))
        {
            sizeScript.StartCoroutine(sizeScript.StartDoingShit());
        }
    }

    public void StopCastingParticles(SpellId spell)
    {
        int slot = (int)spell;

        if (TryGet(castingParticles, slot, out ParticleSystem particles))
        {
            if (TryGet(castingParticleSizeScript, slot, out GetParticleSizeFromCastPercentage sizeScript))
            {
                sizeScript.castingLight.enabled = false;
            }
            particles.Stop();
        }

        if (TryGet(castingSound, slot, out StudioEventEmitter sound)) sound.Stop();
    }

    private static bool InRange(System.Array array, int slot) =>
        array != null && slot >= 0 && slot < array.Length;

    // Unity's null check, so a destroyed reference counts as "this spell has none"
    private static bool TryGet<T>(T[] array, int slot, out T value) where T : Object
    {
        value = InRange(array, slot) ? array[slot] : null;
        return value != null;
    }
}
