using UnityEngine;

public class Bullet : MonoBehaviour
{

    private int damage = 30;

   private AngelMind angelMind;
   [SerializeField] private AiHealth aiHealth;

    void Awake()
    {
        Destroy(gameObject,3);
    }

    private void OnCollisionEnter(Collision collision)
    {


        if (collision.gameObject.CompareTag("Angel")) //Angel Damage
        {
            angelMind = collision.gameObject.GetComponent<AngelMind>();
            angelMind.hp -= damage;

            if (angelMind.hp <= damage) {
                angelMind.isDead = true;
                collision.gameObject.SetActive(false);
            }
        }

        if (collision.gameObject.CompareTag("EvilEntity")) //Evil Dude Damage
        {
            aiHealth = collision.gameObject.GetComponent<AiHealth>();
            aiHealth.hp -= damage;

            if (aiHealth.hp <= damage)
            {
                aiHealth.hp = 0;
                collision.gameObject.SetActive(false);
            }
        }

        Destroy(gameObject);
    }
}

//make it just one damage event, that adapts to the AI that is being shot
