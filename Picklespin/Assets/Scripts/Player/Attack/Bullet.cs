using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Unity.VisualScripting;

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

    private Transform mainCamera;
    private CameraShake cameraShake;

    private Transform handCastingPoint;

    private DamageUI damageUI;
    private AiHealthUiBar aiHealthUI;


    [Header("Misc")]
    public bool iWillBeCritical;
    public bool hitSomething = false;


    void Awake()
    {
        Destroy(gameObject,10);
        originalDamage = damage;
        cameraShake = GameObject.FindGameObjectWithTag("CameraHandler").GetComponent<CameraShake>();
        handCastingPoint = GameObject.FindGameObjectWithTag("CastingPoint").GetComponent<Transform>();
        damageUI = GameObject.FindGameObjectWithTag("DamageUI").GetComponent<DamageUI>();
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
            aiVision = collision.gameObject.GetComponent<AiVision>();
            aiHealthUI = collision.gameObject.GetComponent<AiHealthUiBar>(); //make it execute only when new enemy is hit


            if (aiHealth != null) //Hit Registered
            {
                RandomizeCritical();

                damageUI.myText.enabled = true;
                damageUI.myText.text = ("- " + damage);
                damageUI.whereIshouldGo = collision.transform.position + new Vector3(0, 2.4f, 0);
                damageUI.transform.position = damageUI.whereIshouldGo;
                damageUI.AnimateDamageUI();

                aiHealth.hp -= damage;

                if (aiHealthUI != null) {
                    aiHealthUI.RefreshBar();
                }

                HitGetsYouNoticed();

                if (aiHealth.hp <= 0) //Death
                {
                    aiHealth.hp = 0;
                    aiHealth.deathEvent.Invoke();
                }
            }

        }
        SpawnExplosion();
        if (destroyOnHit) {
            Destroy(gameObject);
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