using FMODUnity;
using System.Collections;
using UnityEngine;

public class SetOnFire : MonoBehaviour
{
    [SerializeField] private int howMuchDamageIdeal = 10;

    [Header("Assets")]
    [SerializeField] private StudioEventEmitter emitter;
    [SerializeField] private Light fireLight;
    [SerializeField] private GameObject diedFromBurnParticle;
    [SerializeField] private ParticleSystem effectParticle;

    private ParticleSystem.EmissionModule particleEmission;
    private ParticleSystem.MainModule particleMain;

    [Header("Debug/Don't touch")]
    [SerializeField][Range(0, 10)] private float countdownTimer = 0;
    private bool imOnFire;
    private bool burned;
    private IEnumerator killer;
    private WaitForSeconds decreaseHpEverySeconds;

    [Header("References")]
    [SerializeField] private AiHealth cachedAiHP;
    [SerializeField] private AiHealthUiBar cachedAiHpBar;
    private DamageUI_Spawner damageUiSpawner;

    private void Awake()
    {
        particleEmission = effectParticle.emission;
        particleMain = effectParticle.main;
    }

    private void OnEnable()
    {
        countdownTimer = 0;

        decreaseHpEverySeconds = new WaitForSeconds(Random.Range(0.45f, 0.55f));

        if (damageUiSpawner == null)
        {
            damageUiSpawner = DamageUI_Spawner.instance;
        }

        Invoke(nameof(FireUp), 0.1f);
    }

    private void Update()
    {
        countdownTimer += Time.deltaTime;
        if (countdownTimer >= 10f)
        {
            if (killer != null)
            {
                StopCoroutine(killer);
            }
            ShutFireDown();
        }
    }

    private void FireUp()
    {
        if (imOnFire) return;

        imOnFire = true;

        if (killer != null)
        {
            StopCoroutine(killer);
        }
        killer = DecreaseHPoverTime();
        StartCoroutine(killer);

        particleEmission.enabled = true;
        effectParticle.Play();
        emitter.Play();
        fireLight.gameObject.SetActive(true);
        fireLight.enabled = true;
    }

    private IEnumerator DecreaseHPoverTime()
    {
        while (true)
        {
            cachedAiHP.hp -= howMuchDamageIdeal;
            RefreshHpBarAndSpawnDamageUI();

            if (cachedAiHP.hp <= 0)
            {
                KillFromFire();
                yield break;
            }

            yield return decreaseHpEverySeconds;
        }
    }

    private void RefreshHpBarAndSpawnDamageUI()
    {
        cachedAiHpBar.RefreshBar();
        damageUiSpawner.Spawn(transform.position + Vector3.up, howMuchDamageIdeal, false);
    }

    public void KillFromFire()
    {
        if (!burned)
        {
            burned = true;
            StartCoroutine(WaitAndKill());
        }
    }

    private IEnumerator WaitAndKill()
    {
        yield return null;
        yield return null;
        yield return null;

        emitter.Stop();
        fireLight.enabled = false;
        fireLight.gameObject.SetActive(false);
        particleEmission.enabled = false;
        effectParticle.Stop();

        diedFromBurnParticle.SetActive(true);
        cachedAiHP.deathEvent.Invoke();
        enabled = false;
    }

    public void ShutFireDown()
    {
        imOnFire = false;
        emitter.Stop();
        fireLight.enabled = false;
        fireLight.gameObject.SetActive(false);
        particleEmission.enabled = false;
        effectParticle.Stop();

        StopAllCoroutines();

        enabled = false;
    }
}
