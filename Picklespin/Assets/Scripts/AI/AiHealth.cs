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

    [Header("Camera shake")]
    [SerializeField] CameraShakeSettings hitShake = new()
    {
        rotationAmount = new Vector3(0.2f, 0.2f, 0.2f), numberOfShakes = 3, speed = 50f, decay = 0.65f, uiShakeModifier = 0f
    };
    [SerializeField] CameraShakeSettings eyeHitShake = new()
    {
        rotationAmount = new Vector3(0.45f, 0.45f, 0.55f), numberOfShakes = 3, speed = 81f, decay = 0f, uiShakeModifier = 1f
    };

    [Header("References")]
    [SerializeField] AiHealthUiBar aiHealthUI;
    [SerializeField] Collider[] myHitboxes;

    [Header("Allied")]
    [SerializeField, Tooltip("what a converted ally takes of every hit - the knob that decides how many it kills before it goes down; see the tuning note in ConvertedAlly")]
    float alliedDamageMultiplier = 0.5f;

    DamageUI_Spawner damageUiSpawner;
    RoundSystem roundSystem;
    CameraShakeManagerV2 camShakeManager;
    float defaultHP;
    bool isDead;

    bool isAllied;

    public bool IsAlive => !isDead;

    public bool CanTakeDamage => !isDead && roundSystem != null && roundSystem.isCounting;

    void Awake()
    {

        defaultHP = hp;
    }

    void Start()
    {
        damageUiSpawner = DamageUI_Spawner.instance;
        camShakeManager = CameraShakeManagerV2.instance;
        roundSystem = RoundSystem.instance;
    }

    float IncomingMultiplier => isAllied ? alliedDamageMultiplier : 1f;

    public void SetAllied(bool allied)
    {
        isAllied = allied;
        if (aiHealthUI && IsAlive) aiHealthUI.SetAllied(allied);
    }

    public void TakeDamage(int damage, bool eyeshot, bool wasLastHitCritical)
    {
        if (!CanTakeDamage) return;

        float actualDamage = (eyeshot ? damage * eyeDamageMultiplier : damage * bodyDamageMultiplier) * IncomingMultiplier;
        if (eyeshot) StartCoroutine(ShakeLater(eyeHitShake));
        else
        {
            camShakeManager.Shake(hitShake);
            eventOnDamageTaken.Invoke();
        }

        hp -= actualDamage;
        if (damageUiSpawner) damageUiSpawner.Spawn(transform.position, (int)actualDamage, wasLastHitCritical);
        RefreshUI();
        CheckIfDead();
    }

    public bool TakeQuietDamage(int damage)
    {
        if (!CanTakeDamage) return false;

        float actualDamage = damage * IncomingMultiplier;
        hp -= actualDamage;
        if (damageUiSpawner) damageUiSpawner.Spawn(transform.position, Mathf.Max(1, (int)actualDamage), false);
        RefreshUI();
        return CheckIfDead();
    }

    public bool WouldDieFrom(int damage) => CanTakeDamage && hp - damage * IncomingMultiplier <= 0;

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

    IEnumerator ShakeLater(CameraShakeSettings settings)
    {
        yield return new WaitForEndOfFrame();
        camShakeManager.Shake(settings);
    }

    public void ResetHealth()
    {
        isDead = false;
        isAllied = false;
        hp = defaultHP;
        for (int i = 0; i < myHitboxes.Length; i++)
            myHitboxes[i].enabled = true;
        RefreshUI();
    }
}
