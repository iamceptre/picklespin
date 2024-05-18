using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Bullet : MonoBehaviour
{
    private int originalDamage;

    [Header("Stats")]
    [SerializeField] private int damage = 15;
    public int magickaCost = 30;
    public int speed = 60;
    public float myCooldown;
    public float castDuration;
    public bool destroyOnHit = true;

    private AiVision aiVision;

    [Header("Assets")]
    [SerializeField] private GameObject spawnInHandParticle;
    [SerializeField] private GameObject explosionFX;
    [SerializeField] private EventReference castSound;
    public EventReference pullupSound;
    [SerializeField] private EventReference hitSound;
    [SerializeField] private EventInstance hitInstance;
    [Tooltip("long casting particle")] public GameObject CastingParticle;

    [Header("Special Effects")]
    [SerializeField] private SetOnFire setOnFire;
    [SerializeField] private bool doesThisSpellSetOnFire = false;

    [Header("References")]
    private AiHealthUiBar aiHealthUI;
    private CameraShake cameraShake;
    private DamageUI_Spawner damageUiSpawner;
    private GiveExpToPlayer giveExpToPlayer;
    [HideInInspector] public Transform handCastingPoint;


    [Header("Misc")]
    [HideInInspector] public bool iWillBeCritical;
    [HideInInspector] public bool hitSomething = false;
    private bool wasLastHitCritical = false;


    void Awake()
    {
        Destroy(gameObject,10);
        originalDamage = damage;
    }

    private void Start()
    {
        damageUiSpawner = DamageUI_Spawner.instance;
        cameraShake = CameraShake.instance;
        RuntimeManager.PlayOneShot(castSound);
        var spawnedCastBlast = Instantiate(spawnInHandParticle, handCastingPoint.position, handCastingPoint.rotation);
        spawnedCastBlast.transform.parent = handCastingPoint;
        transform.localEulerAngles = new Vector3(Random.Range(0,360), Random.Range(0, 360), Random.Range(0, 360));
    }


    private void OnCollisionEnter(Collision collision)
    {
        hitSomething = true;
        if (collision.gameObject)
        {
            hitSomething = true;

            if (collision.gameObject.TryGetComponent<AiHealth>(out AiHealth aiHealth)) //ENEMY HIT REGISTERED
            {
                aiVision = collision.gameObject.GetComponent<AiVision>();
                aiHealthUI = collision.gameObject.GetComponentInChildren<AiHealthUiBar>();
                giveExpToPlayer = collision.gameObject.GetComponent<GiveExpToPlayer>(); //make get components execute only when new enemy has been hit

                RandomizeCritical();

                giveExpToPlayer.wasLastShotAHeadshot = false;

                if (collision.collider.gameObject.transform.CompareTag("Hitbox_Head"))
                {
                    Headshot(collision);
                    giveExpToPlayer.wasLastShotAHeadshot = true;
                }

                aiHealth.hp -= damage;

                damageUiSpawner.Spawn(collision.transform.position, damage, wasLastHitCritical);

                if (aiHealth.hp <= 0) {
                    collision.collider.enabled = false;
                    aiHealth.deathEvent.Invoke();
                }
                else
                {
                    ApplySpecialEffect(collision);
                }

                aiHealthUI.RefreshBar();
                HitGetsYouNoticed();

            }

        }
        SpawnExplosion();
        if (destroyOnHit) {
            Destroy(gameObject);
        }
    }


    private void Headshot(Collision collision)
    {
        damage *= 3;
        ParticleSystem headshotParticle = collision.collider.gameObject.transform.GetComponent<ParticleSystem>();
        headshotParticle.Play();
        //maybe a low-key sound
    }


    private void ApplySpecialEffect(Collision collision)
    {
        if (doesThisSpellSetOnFire)
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
                addedEffect.ResetCountdowns();
            }
        }
    }


    private void RandomizeCritical()
    {
        if (Random.Range(0,10) >= 9 || iWillBeCritical) // 1/10 chance of doubling the damage OR when low on magicka (iWillBeCritical is then set to true by Attack script)
        {
            damage = originalDamage * 4;
            wasLastHitCritical = true;
            RuntimeManager.PlayOneShot("event:/PLACEHOLDER_UNCZ/ohh"); //CRITICAL SOUND
        }
        else
        {
            damage = originalDamage;
            wasLastHitCritical = false;
        }
    }


    private void HitGetsYouNoticed() //make it notice all AIs around
    {
        if (aiVision != null)
        {
            aiVision.hitMeCooldown = 10;
            aiVision.playerJustHitMe = true;
        }
    }

    private void SpawnExplosion()
    {
        Instantiate(explosionFX, Vector3.Lerp(transform.position, Camera.main.transform.position, 0.1f), Quaternion.identity); //prevents explosion clipping through ground

        hitInstance = RuntimeManager.CreateInstance(hitSound);
        FMOD.ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(transform.position);
        hitInstance.set3DAttributes(attributes);

        RuntimeManager.AttachInstanceToGameObject(hitInstance, GetComponent<Transform>());
        cameraShake.ExplosionNearbyShake(Vector3.Distance(transform.position, Camera.main.transform.position),originalDamage);
        hitInstance.start();
    }

}