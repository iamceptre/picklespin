using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SetOnFire : MonoBehaviour
{
    [Header("Assets")]

    public EventReference effectAudio;
    private EventInstance effectAudioInstance;
    public GameObject ParticleObject;
    public GameObject killedByBurnEffect; //both particle and sound

    [Header("Debug/Don't touch")]
    [SerializeField] private ParticleSystem effectParticle;
    private ParticleSystem.MainModule particleMain;
    private bool imOnFire = false;
    private bool killedFromBurning = false;
    private DamageUI damageUI;

    private IEnumerator killer;

    [SerializeField] private int howMuchDamageIdeal = 2;

    private AiHealth cachedAiHP;
    private AiHealthUiBar cachedAiHpBar;
    private EvilEntityDeath cachedDeathScirpt;

    private GameObject spawnedParticle;

    [SerializeField] [Range(0,10)] private float countdownTimer = 0;

    public void StartFire()
    {
        damageUI = DamageUI.instance;
        Invoke("FireUp", 0.2f);
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
            effectAudioInstance = RuntimeManager.CreateInstance(effectAudio);
            effectAudioInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            effectAudioInstance.start();
            AiLinkUp();
        }
        imOnFire = true;
    }


    private void AiLinkUp()
    {
        cachedAiHP = gameObject.GetComponent<AiHealth>();
        cachedAiHpBar = GetComponentInChildren<AiHealthUiBar>();
        cachedDeathScirpt = GetComponentInParent<EvilEntityDeath>();

        killer = DecreaseHPoverTime();
        StopCoroutine(killer);
        StartCoroutine(killer);
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
        if (cachedAiHP.hp <= 0 && !killedFromBurning)
        {
            imOnFire = false;
            effectAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            effectAudioInstance.release();
            cachedDeathScirpt.Die();
            killedFromBurning = true;
        }
    }

    public void PanicKill()
    {
        Debug.Log("Died from burn");
        StopAllCoroutines();
        effectAudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        effectAudioInstance.release();
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

        if (damageUI != null)
        {
            damageUI.gameObject.SetActive(true);
            damageUI.myText.enabled = true;
            damageUI.myText.text = ("- " + howMuchDamageIdeal);
            damageUI.whereIshouldGo = transform.position + new Vector3(0, 2.4f, 0);
            damageUI.transform.position = damageUI.whereIshouldGo;
            damageUI.WhenNotCritical();
            damageUI.AnimateDamageUI();
        }

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
        effectAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        effectAudioInstance.release();
        particleMain.loop = false;

        effectParticle.Stop();
        StopAllCoroutines();
        Destroy(this);
    }

}
