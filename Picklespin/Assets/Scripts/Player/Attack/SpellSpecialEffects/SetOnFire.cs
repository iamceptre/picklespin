using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

// The enabled flag *is* the burning flag: OnEnable lights the fire, OnDisable puts
// it out, so every way a burn can be interrupted cleans itself up for free.
public class SetOnFire : MonoBehaviour
{
    [Header("Burn")]
    [FormerlySerializedAs("howMuchDamageIdeal")]
    [SerializeField] private int damagePerTick = 5;
    [SerializeField, Tooltip("seconds — how long one ignition lasts before burning out")]
    private float burnDuration = 11f;
    [SerializeField, Tooltip("seconds between damage ticks")]
    private float tickInterval = 0.6f;

    [Header("Assets")]
    [SerializeField] private StudioEventEmitter emitter;
    [SerializeField] private Light fireLight;
    [SerializeField] private GameObject diedFromBurnParticle;
    [SerializeField] private ParticleSystem effectParticle;

    [Header("References")]
    [Tooltip("auto-found on a parent if left empty")]
    [SerializeField] private AiHealth cachedAiHP;

    private ParticleSystem.EmissionModule particleEmission;
    private ParticleSystem burnDeathParticle;
    private StudioEventEmitter burnDeathEmitter;
    private Coroutine burnRoutine;
    private WaitForSeconds tickWait;
    private float burnEndsAt;

    // phase offset per ignition: a crowd lit by one blast never ticks on one frame
    private static int igniteCount;

    public bool IsBurning => enabled;

    // latched by Extinguish(), which the death chain calls before reading it
    public bool WasBurningAtDeath { get; private set; }

    private void Awake()
    {
        if (!cachedAiHP) cachedAiHP = GetComponentInParent<AiHealth>(true);
        if (effectParticle) particleEmission = effectParticle.emission;

        if (diedFromBurnParticle)
        {
            burnDeathParticle = diedFromBurnParticle.GetComponentInChildren<ParticleSystem>(true);
            burnDeathEmitter = diedFromBurnParticle.GetComponentInChildren<StudioEventEmitter>(true);

            // ObjectStart fires once per object, so a pooled enemy would burn to death
            // in silence after the first; ShowBurnDeath plays it explicitly instead
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

    // enabled = true on an already-enabled component never re-runs OnEnable, so a
    // re-ignition has to refresh the burn by hand
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

    public void Extinguish()
    {
        WasBurningAtDeath = enabled;
        enabled = false;
    }

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
        tickWait ??= new WaitForSeconds(tickInterval);
        float firstTickAt = Time.time + PhiMath.GoldenSequence(igniteCount++) * tickInterval;
        while (Time.time < firstTickAt) yield return null;

        while (Time.time < burnEndsAt)
        {
            yield return tickWait;

            if (!cachedAiHP.IsAlive) break;
            if (!cachedAiHP.CanTakeDamage) continue;

            // before the damage lands: the death chain calls Extinguish(), which
            // kills this coroutine
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

        // playOnAwake is off on the prefab: activating the object is not enough
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
            else effectParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
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
