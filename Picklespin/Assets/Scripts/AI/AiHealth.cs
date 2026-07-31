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
    bool isDead;

    // the single guard on the death chain, none of which survives running twice
    public bool IsAlive => !isDead;

    public bool CanTakeDamage => !isDead && roundSystem != null && roundSystem.isCounting;

    void Awake()
    {
        // captured in Awake: pooled spawners may call ResetHealth before Start has run
        defaultHP = hp;
    }

    void Start()
    {
        damageUiSpawner = DamageUI_Spawner.instance;
        camShakeManager = CameraShakeManagerV2.instance;
        roundSystem = RoundSystem.instance;
    }

    public void TakeDamage(int damage, bool eyeshot, bool wasLastHitCritical)
    {
        if (!CanTakeDamage) return;

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

    // HP and UI only: the impact reactions would rattle the screen every tick.
    // Returns true if this tick killed.
    public bool TakeBurnDamage(int damage)
    {
        if (!CanTakeDamage) return false;

        hp -= damage;
        if (damageUiSpawner) damageUiSpawner.Spawn(transform.position, damage, false);
        RefreshUI();
        return CheckIfDead();
    }

    // lets a damage source play its death visuals before the chain tears the object down
    public bool WouldDieFrom(int damage) => CanTakeDamage && hp - damage <= 0;

    void RefreshUI()
    {
        if (aiHealthUI) aiHealthUI.RefreshBar();
    }

    bool CheckIfDead()
    {
        if (isDead || hp > 0) return false;

        isDead = true;
        for (int i = 0; i < myHitboxes.Length; i++)
            myHitboxes[i].enabled = false;
        deathEvent.Invoke();
        return true;
    }

    IEnumerator ShakeLater(int index)
    {
        yield return new WaitForEndOfFrame();
        camShakeManager.ShakeSelected(index);
    }

    public void ResetHealth()
    {
        isDead = false;
        hp = defaultHP;
        for (int i = 0; i < myHitboxes.Length; i++)
            myHitboxes[i].enabled = true;
        RefreshUI();
    }
}
