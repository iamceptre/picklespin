using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class Bullet : MonoBehaviour
{

    private int originalDamage;
    [SerializeField] private int damage = 15;
    public int magickaCost = 30;
    public int speed = 60;

    private AiHealth aiHealth;
    private AiVision aiVision;

    [SerializeField] private GameObject explosionFX;
    [SerializeField] private EventReference castSound;
    public EventReference pullupSound;
    [SerializeField] private EventReference hitSound;
    [SerializeField] private EventInstance hitInstance;

    private Transform mainCamera;
    [SerializeField] private CameraShake cameraShake;
    

    void Awake()
    {
        Destroy(gameObject,10);
        originalDamage = damage;
        cameraShake = GameObject.Find("CameraHandler").GetComponent<CameraShake>();
    }

    private void Start()
    {
        RuntimeManager.PlayOneShot(castSound);
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
                    collision.gameObject.SetActive(false);
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
        cameraShake.ExplosionNearbyShake(Vector3.Distance(transform.position, mainCamera.position));
        hitInstance.start();
    }

}