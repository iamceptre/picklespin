using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

// Damage-over-time burn applied by fire spells. Lives on a child of the enemy
// ("LastingSpellEffects/SetOnFire"), driven through Ignite() / Extinguish().
//
// The component's enabled flag *is* the burning flag: OnEnable lights the fire,
// OnDisable puts it out. Every way a burn can be interrupted therefore cleans
// itself up for free — the enemy dying to another spell, the round ending, or a
// pooled enemy being deactivated mid-burn.
//
// It never invokes deathEvent itself. Lethal ticks go through AiHealth so the
// death chain runs exactly once, from one place, with the hitboxes disabled.
public class SetOnFire : MonoBehaviour
{
    [Header("Burn")]
    [FormerlySerializedAs("howMuchDamageIdeal")]
    [SerializeField] private int damagePerTick = 5;
    [SerializeField, Tooltip("φ⁵ seconds — how long one ignition lasts before burning out")]
    private float burnDuration = PhiMath.PHI5;
    [SerializeField, Tooltip("1/φ seconds between damage ticks")]
    private float tickInterval = PhiMath.INV_PHI;

    [Header("Assets")]
    [SerializeField] private StudioEventEmitter emitter;
    [SerializeField] private Light fireLight;
    [SerializeField] private GameObject diedFromBurnParticle;
    [SerializeField] private ParticleSystem effectParticle;

    [Header("References")]
    [Tooltip("auto-found on a parent if left empty")]
    [SerializeField] private AiHealth cachedAiHP; // refreshes the HP bar and the damage numbers itself

    private ParticleSystem.EmissionModule particleEmission;
    private ParticleSystem burnDeathParticle;
    private StudioEventEmitter burnDeathEmitter;
    private Coroutine burnRoutine;
    private WaitForSeconds tickWait;
    private float burnEndsAt;

    // golden-sequence phase offset per ignition: a crowd lit by one blast never
    // ticks (and never spawns its damage numbers) on the same frame
    private static int igniteCount;

    public bool IsBurning => enabled;

    // latched by Extinguish() so the death event can still tell a burnt corpse
    // from a plain one after the fire has already been put out
    public bool WasBurningAtDeath { get; private set; }

    private void Awake()
    {
        // dropping this component onto any enemy is all it should take
        if (!cachedAiHP) cachedAiHP = GetComponentInParent<AiHealth>(true);
        if (effectParticle) particleEmission = effectParticle.emission;

        if (diedFromBurnParticle)
        {
            burnDeathParticle = diedFromBurnParticle.GetComponentInChildren<ParticleSystem>(true);
            burnDeathEmitter = diedFromBurnParticle.GetComponentInChildren<StudioEventEmitter>(true);

            // the prefab plays this off ObjectStart, which only ever fires once —
            // a pooled enemy would burn to death in silence every time after the
            // first. ShowBurnDeath drives it explicitly instead.
            if (burnDeathEmitter) burnDeathEmitter.EventPlayTrigger = EmitterGameEvent.None;
        }
    }

    private void OnEnable()
    {
        if (cachedAiHP == null)
        {
            Debug.LogWarning($"{name}: SetOnFire has no AiHealth reference, cannot burn", this);
            enabled = false;
            return;
        }

        // never light up something that is already dying — the low-HP edge case
        // where the killing blow lands on the same frame as the ignition
        if (!cachedAiHP.IsAlive)
        {
            enabled = false;
            return;
        }

        burnEndsAt = Time.time + burnDuration;
        ShowFire(true);
        burnRoutine = StartCoroutine(BurnRoutine());
    }

    private void OnDisable()
    {
        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            burnRoutine = null;
        }
        ShowFire(false);
    }

    // the entry point spells should use. Re-igniting something that is already
    // burning refreshes the burn instead of silently doing nothing (setting
    // enabled = true on an enabled component never re-runs OnEnable)
    public void Ignite()
    {
        if (cachedAiHP != null && !cachedAiHP.IsAlive) return;

        if (enabled)
        {
            burnEndsAt = Time.time + burnDuration;
            return;
        }

        enabled = true;
    }

    // put the fire out without killing anything. The death chain calls this
    // before dissolving the body, so it latches whether the enemy was still
    // alight — WasBurningAtDeath is what the rest of the death event reads.
    public void Extinguish()
    {
        WasBurningAtDeath = enabled;
        enabled = false; // OnDisable does the cleanup
    }

    // pooled reuse: wipe every trace of the previous burn before the enemy is
    // respawned. Called from AiReferences.ResetAll while the enemy is inactive,
    // so OnDisable has already run — this only clears what it leaves behind.
    public void ResetFireState()
    {
        enabled = false;
        burnRoutine = null;
        burnEndsAt = 0;
        WasBurningAtDeath = false;
        if (effectParticle) effectParticle.Clear(true);
        if (burnDeathParticle) burnDeathParticle.Clear(true);
        if (diedFromBurnParticle) diedFromBurnParticle.SetActive(false);
    }

    private IEnumerator BurnRoutine()
    {
        // one wait object reused for the whole burn and every later ignition;
        // the stagger is folded into the first tick rather than being its own
        // throwaway WaitForSeconds
        tickWait ??= new WaitForSeconds(tickInterval);
        float firstTickAt = Time.time + PhiMath.GoldenSequence(igniteCount++) * tickInterval;
        while (Time.time < firstTickAt) yield return null;

        while (Time.time < burnEndsAt)
        {
            yield return tickWait;

            // dead, or the round ended: stop chewing HP but let the burn expire
            if (!cachedAiHP.IsAlive) break;
            if (!cachedAiHP.CanTakeDamage) continue;

            // the burn-death visuals have to go up *before* the damage lands —
            // the death chain calls Extinguish(), which kills this coroutine
            if (cachedAiHP.WouldDieFrom(damagePerTick)) ShowBurnDeath();

            cachedAiHP.TakeBurnDamage(damagePerTick);
            if (!cachedAiHP.IsAlive) break;
        }

        burnRoutine = null;
        enabled = false;
    }

    private void ShowBurnDeath()
    {
        ShowFire(false);
        if (!diedFromBurnParticle) return;

        // playOnAwake is off on this prefab, so activating the object is not
        // enough — the burst has to be fired by hand, every death
        diedFromBurnParticle.SetActive(true);
        if (burnDeathParticle)
        {
            burnDeathParticle.Clear(true);
            burnDeathParticle.Play(true);
        }
        if (burnDeathEmitter) burnDeathEmitter.Play();
    }

    private void ShowFire(bool on)
    {
        if (effectParticle)
        {
            particleEmission.enabled = on;
            if (on) effectParticle.Play();
            else effectParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting); // let the last flames fade
        }

        if (fireLight)
        {
            fireLight.gameObject.SetActive(on);
            fireLight.enabled = on;
        }

        if (emitter)
        {
            if (on) emitter.Play();
            else emitter.Stop();
        }
    }
}
