using UnityEngine;

public class Bullet : MonoBehaviour
{

    private int damage = 30;

   private AngelMind angelMind;
   [SerializeField]private EvilEntityMind evilEntityMind;

    void Awake()
    {
        Destroy(gameObject,3);
    }

    private void OnCollisionEnter(Collision collision)
    {


        if (collision.gameObject.CompareTag("Angel")) //Angel Damage
        {
            angelMind = collision.gameObject.GetComponent<AngelMind>();
          //  Debug.Log("Hit Angel");
            angelMind.hp -= damage;

            if (angelMind.hp <= damage) {
                angelMind.isDead = true;
                collision.gameObject.SetActive(false);
            }
        }

        if (collision.gameObject.CompareTag("EvilEntity")) //Evil Dude Damage
        {
            evilEntityMind = collision.gameObject.GetComponent<EvilEntityMind>();
         //   Debug.Log("Hit Evil Dude");
            evilEntityMind.hp -= damage;

            if (evilEntityMind.hp <= damage)
            {
                evilEntityMind.isDead = true;
                collision.gameObject.SetActive(false);
            }
        }

        Destroy(gameObject);
    }
}

//make it just one damage event, that adapts to the AI that is being shot
