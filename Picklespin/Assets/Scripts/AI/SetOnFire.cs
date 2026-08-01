using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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

    private static int igniteCount;

    public bool IsBurning => enabled;

    public bool WasBurningAtDeath { get; private set; }

    private void Awake()
    {
        if (!cachedAiHP) cachedAiHP = GetComponentInParent<AiHealth>(true);
        if (effectParticle) particleEmission = effectParticle.emission;

        if (diedFromBurnParticle)
        {
            burnDeathParticle = diedFromBurnParticle.GetComponentInChildren<ParticleSystem>(true);
            burnDeathEmitter = diedFromBurnParticle.GetComponentInChildren<StudioEventEmitter>(true);

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

            if (cachedAiHP.WouldDieFrom(damagePerTick)) ShowBurnDeath();

            cachedAiHP.TakeQuietDamage(damagePerTick);
            if (!cachedAiHP.IsAlive) break;
        }

        burnRoutine = null;
        enabled = false;
    }

    private void ShowBurnDeath()
    {
        ShowFire(false);
        if (!diedFromBurnParticle) return;

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
