using UnityEngine;
using UnityEngine.AI;

public class Bullet : MonoBehaviour
{

    private int originalDamage;
    private int damage = 15;

    private AiHealth aiHealth;
    private AiVision aiVision;

    [SerializeField] private GameObject explosionFX;

    void Awake()
    {
        Destroy(gameObject,10);
        originalDamage = damage;
    }

    private void OnCollisionEnter(Collision collision)
    {

        RandomizeCritical(); 

        //Debug.Log("you deal " + damage + " damage");
        if (collision.gameObject) //Evil Dude Damage
        {
            aiHealth = collision.gameObject.GetComponent<AiHealth>();

            aiVision = collision.gameObject.GetComponent<AiVision>();

            if (aiHealth != null)
            {
                aiHealth.hp -= damage;
                HitGetsYouNoticed();

                if (aiHealth.hp <= damage)
                {
                    aiHealth.hp = 0; //Death
                    collision.gameObject.SetActive(false);
                }
            }

        }
        Instantiate(explosionFX, transform.position, Quaternion.identity);
        Destroy(gameObject);

    }


    private void RandomizeCritical()
    {
        if (Random.Range(0,10) >= 9) // 1/10 chance of critical
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