using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Bullet : MonoBehaviour
{

    private int originalDamage;
    [SerializeField] private int damage = 15;
    public int magickaCost = 30;
    public int speed = 60;
    public float myCooldown;

    private AiHealth aiHealth;
    private AiVision aiVision;

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

    public float castDuration;


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
        if (collision.gameObject)
        {
            damageUI.gameObject.SetActive(true);
            aiHealth = collision.gameObject.GetComponent<AiHealth>();
            aiVision = collision.gameObject.GetComponent<AiVision>();

            if (aiHealth != null) //Hit Registered
            {
                RandomizeCritical();
                damageUI.myText.enabled = true;
                damageUI.myText.text = ("- " + damage);
               // damageUI.whoHasBeenHit = collision.gameObject.transform;
                damageUI.whereIshouldGo = collision.transform.position + new Vector3(0, 2.4f, 0);
                damageUI.transform.position = damageUI.whereIshouldGo;
                damageUI.AnimateDamageUI();
                aiHealth.hp -= damage;
                HitGetsYouNoticed();

                if (aiHealth.hp <= 0) //Death
                {
                    aiHealth.hp = 0;
                    aiHealth.deathEvent.Invoke();
                }
            }

        }
        SpawnExplosion();
        Destroy(gameObject);
    }


    private void RandomizeCritical()
    {
        if (Random.Range(0,10) >= 9) // 1/10 chance of doubling the damage
        {
            damage = originalDamage * 2;
            damageUI.WhenCritical();
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
        RuntimeManager.AttachInstanceToGameObject(hitInstance, GetComponent<Transform>());
        cameraShake.ExplosionNearbyShake(Vector3.Distance(transform.position, mainCamera.position),originalDamage);
        hitInstance.start();
    }

}