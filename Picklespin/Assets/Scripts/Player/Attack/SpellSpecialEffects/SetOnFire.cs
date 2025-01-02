using FMODUnity;
using System.Collections;
using UnityEngine;

public class SetOnFire : MonoBehaviour
{
    [SerializeField] private int howMuchDamageIdeal = 10;

    [Header("Assets")]
    [SerializeField] private StudioEventEmitter emitter;
    [SerializeField] private Light fireLight;
    [SerializeField] private GameObject diedFromBurnParticle; //both particle and sound


    [SerializeField] private ParticleSystem effectParticle;
    private ParticleSystem.EmissionModule particleEmission;
    private ParticleSystem.MainModule particleMain;

    [Header("Debug/Don't touch")]
    private bool imOnFire = false;
    private bool burned = false;
    private IEnumerator killer;

    [Header("References")]
    [SerializeField] private AiHealth cachedAiHP;
    [SerializeField] private AiHealthUiBar cachedAiHpBar;
    private DamageUI_Spawner damageUiSpawner;

    [SerializeField][Range(0, 10)] private float countdownTimer = 0;

    private WaitForSeconds decreaseHpEverySeconds;


    private void Awake()
    {
        particleEmission = effectParticle.emission;
        particleMain = effectParticle.main;
    }


    private void OnEnable()
    {
        ResetCountdowns();

        decreaseHpEverySeconds = new WaitForSeconds(Random.Range(0.45f, 0.55f));

        if (damageUiSpawner == null)
        {
            damageUiSpawner = DamageUI_Spawner.instance;
        }

        Invoke("FireUp", 0.1f);
    }


    public void ResetCountdowns()
    {
        countdownTimer = 0;
    }

    private void FireUp()
    {
        if (!imOnFire)
        {
            imOnFire = true;
            killer = DecreaseHPoverTime();
            StopCoroutine(killer);
            StartCoroutine(killer);

            particleEmission.enabled = true;
            effectParticle.Play();

            emitter.Play();
            fireLight.gameObject.SetActive(true);
            fireLight.enabled = true;
        }
    }


    private IEnumerator DecreaseHPoverTime()
    {
        while (true)
        {
            cachedAiHP.hp -= howMuchDamageIdeal;
            AiHpBarRefresher();

            if (cachedAiHP.hp <= 0)
            {
                KillFromFire(); //die from fire
            }

            yield return decreaseHpEverySeconds;
        }
    }

    public void KillFromFire() //spalenie
    {
        if (!burned)
        {
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
        burned = true;
        particleEmission.enabled = false;
        effectParticle.Stop();
        diedFromBurnParticle.SetActive(true);
        cachedAiHP.deathEvent.Invoke();
        enabled = false;
    }

    private void AiHpBarRefresher()
    {
        cachedAiHpBar.RefreshBar();
        damageUiSpawner.Spawn(transform.position + new Vector3(0, 1), howMuchDamageIdeal, false); //maybe a different color
    }


    private void Update()
    {
        countdownTimer += Time.deltaTime;

        if (countdownTimer >= 10)
        {
            if (killer != null)
            {
                StopCoroutine(killer);
            }

            ShutFireDown();
        }
    }

    public void ShutFireDown() //ugaszenie prze�ywaj�c
    {
        particleEmission.enabled = false;
        imOnFire = false;
        emitter.Stop();
        fireLight.enabled = false;
        fireLight.gameObject.SetActive(false);
        effectParticle.Stop();
        StopAllCoroutines();
        enabled = false;
    }

}
