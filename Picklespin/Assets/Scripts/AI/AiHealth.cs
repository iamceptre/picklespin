using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AiHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Range(0, 100)] public float hp = 100;
    [SerializeField] float bodyDamageMultiplier = 1f;
    [SerializeField] float eyeDamageMultiplier = 4f;

    [Header("Events")]
    public UnityEvent deathEvent;
    [SerializeField] UnityEvent eventOnDamageTaken;

    [Header("References")]
    [SerializeField] AiHealthUiBar aiHealthUI;
    [SerializeField] Collider[] myHitboxes;

    DamageUI_Spawner damageUiSpawner;
    RoundSystem roundSystem;
    CameraShakeManagerV2 camShakeManager;
    float defaultHP;

    void Start()
    {
        damageUiSpawner = DamageUI_Spawner.instance;
        camShakeManager = CameraShakeManagerV2.instance;
        roundSystem = RoundSystem.instance;
        defaultHP = hp;
    }

    public void TakeDamage(int damage, bool eyeshot, bool wasLastHitCritical)
    {
        if (!roundSystem.isCounting) return;

        float actualDamage = eyeshot ? damage * eyeDamageMultiplier : damage * bodyDamageMultiplier;
        if (eyeshot) StartCoroutine(ShakeLater(3));
        else
        {
            camShakeManager.ShakeSelected(2);
            eventOnDamageTaken.Invoke();
        }

        hp -= actualDamage;
        if (damageUiSpawner) damageUiSpawner.Spawn(transform.position, (int)actualDamage, wasLastHitCritical);
        RefreshUI();
        CheckIfDead();
    }

    void RefreshUI()
    {
        if (aiHealthUI) aiHealthUI.RefreshBar();
    }

    void CheckIfDead()
    {
        if (hp <= 0)
        {
            for (int i = 0; i < myHitboxes.Length; i++)
                myHitboxes[i].enabled = false;
            deathEvent.Invoke();
        }
    }

    IEnumerator ShakeLater(int index)
    {
        yield return new WaitForEndOfFrame();
        camShakeManager.ShakeSelected(index);
    }

    public void ResetHealth()
    {
        hp = defaultHP;
        for (int i = 0; i < myHitboxes.Length; i++)
            myHitboxes[i].enabled = true;
        RefreshUI();
    }
}
