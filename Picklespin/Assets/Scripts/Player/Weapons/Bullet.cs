using UnityEngine;
using FMODUnity;

public class Bullet : MonoBehaviour
{

    private int originalDamage;
    [SerializeField] private int damage = 15;
    public int magickaCost = 30;
    public int speed = 60;

    private AiHealth aiHealth;
    private AiVision aiVision;

    [SerializeField] private GameObject explosionFX;
    [SerializeField] private EventReference mySound;

    void Awake()
    {
        Destroy(gameObject,10);
        originalDamage = damage;
    }

    private void Start()
    {
        RuntimeManager.PlayOneShot(mySound);
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
        Instantiate(explosionFX, transform.position, Quaternion.identity);
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

}