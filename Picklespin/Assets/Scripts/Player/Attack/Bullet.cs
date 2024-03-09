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
    //[SerializeField] private float turbulenceStrength = 1f;


    void Awake()
    {
        Destroy(gameObject,10);
        originalDamage = damage;
        cameraShake = GameObject.Find("CameraHandler").GetComponent<CameraShake>();
        handCastingPoint = GameObject.FindGameObjectWithTag("CastingPoint").GetComponent<Transform>();
    }

    private void Start()
    {
        RuntimeManager.PlayOneShot(castSound);
        Instantiate(spawnInHandParticle, handCastingPoint.position, handCastingPoint.rotation);
    }


    private void OnCollisionEnter(Collision collision)
    {

        RandomizeCritical(); 
        //Debug.Log("you deal " + damage + " damage");

        if (collision.gameObject)
        {
            aiHealth = collision.gameObject.GetComponent<AiHealth>();
            aiVision = collision.gameObject.GetComponent<AiVision>();

            if (aiHealth != null)
            {
                aiHealth.hp -= damage;
                HitGetsYouNoticed();

                if (aiHealth.hp <= damage) //Death
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
        }
        else
        {
            damage = originalDamage;
        }
    }


    private void HitGetsYouNoticed()
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











/*
    private void Turbulence()
    {
        float turbulenceFreq;
        turbulenceFreq = 15124;

        // Generate random turbulence force
        Vector3 turbulenceForce = new Vector3(
            Mathf.PerlinNoise(Time.time * turbulenceFreq, 0) - 0.5f,
            Mathf.PerlinNoise(0, Time.time * turbulenceFreq) - 0.5f,
            0
        ) * turbulenceStrength;

        // Apply turbulence force to Rigidbody
        GetComponent<Rigidbody>().AddForce(turbulenceForce);
    }

  */