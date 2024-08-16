using UnityEngine;
using UnityEngine.Events;

public class AiHealth : MonoBehaviour
{
    public UnityEvent deathEvent;
    [Range(0, 100)] public float hp = 100;
    [SerializeField] private float bodyDamageMultiplier = 1.0f;
    [SerializeField] private float eyeDamageMultiplier = 4.0f;
    private DamageUI_Spawner damageUiSpawner;
    [SerializeField] private AiHealthUiBar aiHealthUI;

    [SerializeField] private UnityEvent eventOnDamageTaken;
    [SerializeField] private UnityEvent eventOnDamageTakenEye;

    [SerializeField] private Collider[] myHitboxes;

    private void Start()
    {
        damageUiSpawner = DamageUI_Spawner.instance;
    }



    public void TakeDamage(int damage, bool eyeshot, bool wasLastHitCritical)
    {
        float actualDamage;

        if (eyeshot)
        {
            actualDamage = damage * eyeDamageMultiplier;
        }
        else
        {
            eventOnDamageTaken.Invoke();
            actualDamage = damage * bodyDamageMultiplier;
        }

        hp -= actualDamage;
        SpawnDamageNumbers((int)actualDamage, wasLastHitCritical);
        RefreshUI();
        CheckIfDead();
    }



    public void SpawnDamageNumbers(int damageTaken, bool wasLastHitCritical)
    {
        damageUiSpawner.Spawn(transform.position, damageTaken, wasLastHitCritical);
    }


    private void RefreshUI()
    {
        if (aiHealthUI != null)
        {
            aiHealthUI.RefreshBar();
        }
    }


    private void CheckIfDead()
    {
        if (hp <= 0)
        {
            for (int i = 0; i < myHitboxes.Length; i++)
            {
                myHitboxes[i].enabled = false;
            }

            deathEvent.Invoke();
        }

    }

}
