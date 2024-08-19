using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AiHealth : MonoBehaviour
{
    public UnityEvent deathEvent;
    [Range(0, 100)] public float hp = 100;
    [SerializeField] private float bodyDamageMultiplier = 1.0f;
    [SerializeField] private float eyeDamageMultiplier = 4.0f;
    private DamageUI_Spawner damageUiSpawner;
    private RoundSystem roundSystem;
    [SerializeField] private AiHealthUiBar aiHealthUI;

    [SerializeField] private UnityEvent eventOnDamageTaken;
   // [SerializeField] private UnityEvent eventOnDamageTakenEye;

    [SerializeField] private Collider[] myHitboxes;

    private CameraShakeManagerV2 camShakeManager;

    private void Start()
    {
        damageUiSpawner = DamageUI_Spawner.instance;
        camShakeManager = CameraShakeManagerV2.instance;
        roundSystem = RoundSystem.instance;
    }



    public void TakeDamage(int damage, bool eyeshot, bool wasLastHitCritical)
    {
        if (roundSystem.isCounting)
        {
            float actualDamage;

            if (eyeshot)
            {
                StartCoroutine(ShakeLater(3));
                actualDamage = damage * eyeDamageMultiplier;
            }
            else
            {
                camShakeManager.ShakeSelected(2);
                actualDamage = damage * bodyDamageMultiplier;
                eventOnDamageTaken.Invoke();
            }

            hp -= actualDamage;
            SpawnDamageNumbers((int)actualDamage, wasLastHitCritical);
            RefreshUI();
            CheckIfDead();
        }
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
        if ((int)hp <= 0)
        {
            for (int i = 0; i < myHitboxes.Length; i++)
            {
                myHitboxes[i].enabled = false;
            }

            deathEvent.Invoke();
        }

    }

    private IEnumerator ShakeLater(int index)
    {
        yield return new WaitForEndOfFrame();
        camShakeManager.ShakeSelected(index);
    }

}
