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
    private bool killedFromBurning = false;

    private IEnumerator killer;

    [SerializeField] private int howMuchDamageIdeal = 2;

    private AiHealth cachedAiHP;
    private AiHealthUiBar cachedAiHpBar;
    private EvilEntityDeath cachedDeathScirpt;
    private DamageUI_Spawner damageUiSpawner;

    private GameObject spawnedParticle;

    [SerializeField][Range(0, 10)] private float countdownTimer = 0;

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
            cachedDeathScirpt = GetComponentInParent<EvilEntityDeath>();
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
            KillFromFire();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void KillFromFire()
    {
        if (cachedAiHP.hp <= 0 && !killedFromBurning)
        {
            imOnFire = false;
            emitter.Stop();
            cachedDeathScirpt.Die();
            killedFromBurning = true;
        }
    }

    public void PanicKill()
    {
        StopAllCoroutines();
        emitter.Stop();
        transform.parent = null;
        Instantiate(killedByBurnEffect, transform.position - new Vector3(0, 2), Quaternion.identity);
        Destroy(gameObject);
    }

    private void AiHpBarRefresher()
    {
        if (cachedAiHpBar != null)
        {
            cachedAiHpBar.RefreshBar();
        }

        damageUiSpawner.Spawn(transform.position + new Vector3(0, 1), howMuchDamageIdeal, false);
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

    public void ShutFireDown()
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
