using System.Collections;
using UnityEngine;
using FMODUnity;

public class SetOnFire : MonoBehaviour
{
    [Header("Assets")]
    public EventReference effectAudio;
    private StudioEventEmitter emitter;
    public GameObject ParticleObject;
    public GameObject killedByBurnEffect; //both particle and sound

    [Header("Debug/Don't touch")]
    [SerializeField] private ParticleSystem effectParticle;
    private ParticleSystem.MainModule particleMain;
    private bool imOnFire = false;
    private bool burned = false;

    private IEnumerator killer;

    [SerializeField] private int howMuchDamageIdeal = 10;

    [Header("References")]
    private AiHealth cachedAiHP;
    private AiHealthUiBar cachedAiHpBar;
    private DamageUI_Spawner damageUiSpawner;

    private GameObject spawnedParticle;

    [SerializeField][Range(0, 10)] private float countdownTimer = 0;

    private WaitForSeconds decreaseHpEverySeconds;


    private void Awake()
    {
        float rand = Random.Range(0.45f, 0.55f);
        decreaseHpEverySeconds = new WaitForSeconds(rand);
    }
    public void StartFire()
    {
        damageUiSpawner = DamageUI_Spawner.instance;
        Invoke("FireUp", 0.1f);
    }

    public void ResetCountdowns()
    {
        countdownTimer = 0;
    }

    private void FireUp()
    {
        spawnedParticle = Instantiate(ParticleObject, transform.position, Quaternion.identity);
        spawnedParticle.transform.parent = transform;
        effectParticle = spawnedParticle.GetComponentInChildren<ParticleSystem>();
        particleMain = effectParticle.main;
        particleMain.loop = true;

        if (!imOnFire)
        {
            SetUpAudio();
            emitter.Play();
            AiLinkUp();
        }
        imOnFire = true;
    }


    private void SetUpAudio()
    {
        emitter = gameObject.GetComponentInChildren<StudioEventEmitter>();
        emitter.EventReference = effectAudio;
    }



    private void AiLinkUp()
    {
        if (cachedAiHP == null)
        {
            cachedAiHP = gameObject.GetComponent<AiHealth>();
            cachedAiHpBar = GetComponentInChildren<AiHealthUiBar>();
        }

        killer = DecreaseHPoverTime();
        StopCoroutine(killer);
        StartCoroutine(killer);
    }

    private IEnumerator DecreaseHPoverTime()
    {
        while (true)
        {
            cachedAiHP.hp -= howMuchDamageIdeal;
            AiHpBarRefresher();

            if (cachedAiHP.hp<=0)
            {
                KillFromFire(); //die from fire
            }

            yield return decreaseHpEverySeconds;
        }
    }

    public void KillFromFire() //spalenie
    {
        if (!burned) {
            burned = true;
            transform.parent = null;
            emitter.Stop();
            particleMain.loop = false;
            effectParticle.Stop();
            Instantiate(killedByBurnEffect, transform.position - new Vector3(0, 2), Quaternion.identity);
            cachedAiHP.deathEvent.Invoke();
            Destroy(this);
        }
    }


    private void AiHpBarRefresher()
    {
        if (cachedAiHpBar != null)
        {
            cachedAiHpBar.RefreshBar();
        }

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

    public void ShutFireDown() //ugaszenie
    {
        transform.parent = null;
        imOnFire = false;
        emitter.Stop();
        particleMain.loop = false;
        effectParticle.Stop();
        StopAllCoroutines();
        Destroy(this);
    }

}
