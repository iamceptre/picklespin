using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SetOnFire : MonoBehaviour
{
    private DamageUI damageUI;
    public EventReference effectAudio;
    private EventInstance effectAudioInstance;

    public GameObject particleObject;
    private GameObject spawnedParticlesObject;
    private ParticleSystem effectParticle;

    private IEnumerator fireCooldownRoutine;
    private IEnumerator killer;

    [SerializeField] private int howMuchDamageIdeal = 2;



    private AiHealth cachedAiHP;
    private AiHealthUiBar cachedAiHpBar;
    private EvilEntityDeath cachedDeathScirpt;

    private void Awake()
    {
        fireCooldownRoutine = FireCooldown();
    }

    void Start()
    {
        damageUI = DamageUI.instance;
        FireUp();
    }

    public void FireUp() //call it when got hit
    {
        StopCoroutine(fireCooldownRoutine);
        StartCoroutine(fireCooldownRoutine);

        if (spawnedParticlesObject == null)
        {
            spawnedParticlesObject = Instantiate(particleObject, transform.position, Quaternion.identity);
            spawnedParticlesObject.transform.parent = transform;
            effectParticle = spawnedParticlesObject.GetComponent<ParticleSystem>();

            effectAudioInstance = RuntimeManager.CreateInstance(effectAudio);
            effectAudioInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            effectAudioInstance.start();
        }

        AiLinkUp();
    }


    private void AiLinkUp()
    {
        if (TryGetComponent<AiHealth>(out AiHealth aiHealth))
        {
            cachedAiHP = aiHealth;
            killer = DecreaseHPoverTime();
            StartCoroutine(killer);


            if(TryGetComponent<EvilEntityDeath>(out EvilEntityDeath deathScript))
            {
                cachedDeathScirpt = deathScript;
            }

            cachedAiHpBar = gameObject.GetComponentInChildren<AiHealthUiBar>();

        }

    }

    private IEnumerator DecreaseHPoverTime()
    {
        while (true)
        {
            cachedAiHP.hp -= howMuchDamageIdeal;
            effectAudioInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            AiHpBarRefresher();
            KillFromFire();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void KillFromFire()
    {
        if (cachedAiHP.hp <= 1) { 
            cachedDeathScirpt.Die();
        }
    }

    private void AiHpBarRefresher()
    {
        if (cachedAiHpBar != null) {
            cachedAiHpBar.RefreshBar();
        }

        if (damageUI != null)
        {
            damageUI.gameObject.SetActive(true);
            damageUI.myText.enabled = true; 
            damageUI.myText.text = ("- " + howMuchDamageIdeal);
            damageUI.whereIshouldGo =  transform.position + new Vector3(0, 2.4f, 0);
            damageUI.transform.position = damageUI.whereIshouldGo;
            damageUI.AnimateDamageUI();
        }

    }

    private IEnumerator FireCooldown()
    {
        yield return new WaitForSeconds(10);

        if (killer != null) {
            StopCoroutine(killer);
        }

        ShutFireDown();
    }

    public void ShutFireDown()
    {
        var mainModule = effectParticle.main;
        var emissionModule = effectParticle.emission;

        mainModule.loop = false;
        emissionModule.enabled = false;

        spawnedParticlesObject.transform.parent = null;

        effectParticle.Stop();

        effectAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        effectAudioInstance.release();

        StopAllCoroutines();
    }

}
