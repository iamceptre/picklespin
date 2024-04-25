using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Bullet : MonoBehaviour
{

    public GameObject CastingParticle;

    private int originalDamage;

    [Header("Stats")]
    [SerializeField] private int damage = 15;
    public int magickaCost = 30;
    public int speed = 60;
    public float myCooldown;
    public float castDuration;
    public bool destroyOnHit = true;

    private AiHealth aiHealth;
    private AiVision aiVision;

    [Header("Assets")]
    [SerializeField] private GameObject spawnInHandParticle;
    [SerializeField] private GameObject explosionFX;
    [SerializeField] private EventReference castSound;
    public EventReference pullupSound;
    [SerializeField] private EventReference hitSound;
    [SerializeField] private EventInstance hitInstance;

    [Header("Special Effects")]
    [SerializeField] private SetOnFire setOnFire;

    private Transform mainCamera;
    private CameraShake cameraShake;

    private Transform handCastingPoint;

    private DamageUI damageUI;
    private AiHealthUiBar aiHealthUI;


    [Header("Misc")]
    [HideInInspector] public bool iWillBeCritical;
    [HideInInspector] public bool hitSomething = false;


    void Awake()
    {
        Destroy(gameObject,10);
        originalDamage = damage;
        cameraShake = GameObject.FindGameObjectWithTag("CameraHandler").GetComponent<CameraShake>();
        handCastingPoint = GameObject.FindGameObjectWithTag("CastingPoint").GetComponent<Transform>();
        damageUI = DamageUI.instance;
    }

    private void Start()
    {
        RuntimeManager.PlayOneShot(castSound);
        Instantiate(spawnInHandParticle, handCastingPoint.position, handCastingPoint.rotation);
        transform.localEulerAngles = new Vector3(Random.Range(0,360), Random.Range(0, 360), Random.Range(0, 360));
    }


    private void OnCollisionEnter(Collision collision)
    {
        hitSomething = true;
        if (collision.gameObject)
        {
            hitSomething = true;
            damageUI.gameObject.SetActive(true);


            aiHealth = collision.gameObject.GetComponent<AiHealth>();


            if (aiHealth != null) //Hit Registered
            {
                aiVision = collision.gameObject.GetComponent<AiVision>();
                aiHealthUI = collision.gameObject.GetComponentInChildren<AiHealthUiBar>();

                RandomizeCritical();
                DamageUIText(collision);

                aiHealth.hp -= damage;

                if (aiHealth.hp <= 0) {
                    collision.collider.enabled = false;
                    aiHealth.deathEvent.Invoke();
                }
                else
                {
                    ApplySpecialEffect(collision);
                }

                if (aiHealthUI != null) {
                    aiHealthUI.RefreshBar();
                }

                HitGetsYouNoticed();

            }

        }
        SpawnExplosion();
        if (destroyOnHit) {
            Destroy(gameObject);
        }
    }


    private void DamageUIText(Collision collision)
    {
        damageUI.myText.enabled = true; // CHANGE THIS BULLSHIT PLEASE
        damageUI.myText.text = ("- " + damage);
        damageUI.whereIshouldGo = collision.transform.position + new Vector3(0, 2.4f, 0);
        damageUI.transform.position = damageUI.whereIshouldGo;
        damageUI.AnimateDamageUI();
    }

    private void ApplySpecialEffect(Collision collision)
    {
        if (setOnFire != null)
        {
            var addedEffect = collision.gameObject.GetComponent<SetOnFire>();

            if (addedEffect == null)
            {
                addedEffect = collision.gameObject.AddComponent<SetOnFire>();
                addedEffect.effectAudio = setOnFire.effectAudio;
                addedEffect.ParticleObject = setOnFire.ParticleObject;
                addedEffect.killedByBurnEffect = setOnFire.killedByBurnEffect;
                addedEffect.StartFire();
            }
            else
            {
                addedEffect.ResetCountdowns(); //FIX THIS REMOVNIG THE OBJECT COMPLETELY
            }
        }
    }


    private void RandomizeCritical()
    {
        if (Random.Range(0,10) >= 9 || iWillBeCritical) // 1/10 chance of doubling the damage OR when low on magicka (iWillBeCritical is then set to true by Attack script)
        {
            damage = originalDamage * 4;
            damageUI.WhenCritical();
            RuntimeManager.PlayOneShot("event:/PLACEHOLDER_UNCZ/ohh"); //CRITICAL SOUND
        }
        else
        {
            damage = originalDamage;
            damageUI.WhenNotCritical();
        }
    }


    private void HitGetsYouNoticed() //make it notice all AIs around
    {
        if (aiVision != null)
        {
            aiVision.hitMeCooldown = 10;
            aiVision.playerJustHitMe = true;
            //aiVision.PlayerJustHitMeCooldown();
        }
    }

    private void SpawnExplosion()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        Instantiate(explosionFX, Vector3.Lerp(transform.position, mainCamera.position, 0.1f), Quaternion.identity); //prevents explosion clipping through ground

        hitInstance = RuntimeManager.CreateInstance(hitSound);
        FMOD.ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(transform.position);
        hitInstance.set3DAttributes(attributes);

        RuntimeManager.AttachInstanceToGameObject(hitInstance, GetComponent<Transform>());
        cameraShake.ExplosionNearbyShake(Vector3.Distance(transform.position, mainCamera.position),originalDamage);
        hitInstance.start();
    }

}