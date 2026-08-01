using UnityEngine;
using FMODUnity;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using FMOD.Studio;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    Animator handAnimator;
    public static Attack instance { get; private set; }
    PlayCastBlast playCastBlast;
    SpellProjectileSpawner spellProjectileSpawner;
    [SerializeField] UnityEvent changeSelectedSpell;
    Ammo ammo;
    AmmoDisplay ammoDisplay;
    [SerializeField] EventReference shootFailEvent;
    EventInstance pullupEventInstance;
    EventInstance spellcastInstance;
    public Bullet[] bulletPrefab;
    public int selectedBulletIndex;
    public float castCooldownTime = 0.1f;
    public bool castCooldownAllow = true;
    [SerializeField] UnlockedSpells unlockedSpells;
    [SerializeField] SpellCooldown spellCooldown;
    [SerializeField] NoManaLightAnimation noManaLightAnimation;
    public float castingProgress = 0;

    float CurrentCastDuration => currentCasting ? currentCasting.Duration * WishUpgrades.CastDurationMultiplier : 0f;
    int CurrentMagickaCost => currentBullet
        ? Mathf.Max(1, Mathf.RoundToInt(currentBullet.magickaCost * WishUpgrades.MagickaCostMultiplier))
        : 0;
    [SerializeField] Slider castingSlider;
    [SerializeField] UnityEvent castingCompleted;
    [SerializeField] UnityEvent CancelCasting;
    public Bullet currentBullet;

    SpellCameraShake currentShake;
    SpellCasting currentCasting;
    [SerializeField] InputActionReference primaryAction;
    [SerializeField] InputActionReference secondaryAction;
    bool isPrimaryPressed;
    bool isSecondaryPressed;
    Coroutine castingRoutine;

    void Awake()
    {
        SetCurrentBullet(selectedBulletIndex);
        if (instance != null && instance != this) Destroy(this); else instance = this;
    }

    void SetCurrentBullet(int index)
    {
        currentBullet = bulletPrefab[index];
        currentShake = currentBullet ? currentBullet.GetComponent<SpellCameraShake>() : null;
        currentCasting = currentBullet ? currentBullet.GetComponent<SpellCasting>() : null;
    }

    void Start()
    {
        handAnimator = PublicPlayerHandAnimator.instance._animator;
        ammo = Ammo.instance;
        playCastBlast = PlayCastBlast.instance;
        ammoDisplay = AmmoDisplay.instance;
        spellProjectileSpawner = SpellProjectileSpawner.instance;
    }

    void OnEnable()
    {
        primaryAction.action.performed += OnPrimaryPerformed;
        primaryAction.action.canceled += OnPrimaryCanceled;
        primaryAction.action.Enable();
        secondaryAction.action.performed += OnSecondaryPerformed;
        secondaryAction.action.canceled += OnSecondaryCanceled;
        secondaryAction.action.Enable();
    }

    void OnDisable()
    {
        primaryAction.action.performed -= OnPrimaryPerformed;
        primaryAction.action.canceled -= OnPrimaryCanceled;
        primaryAction.action.Disable();
        secondaryAction.action.performed -= OnSecondaryPerformed;
        secondaryAction.action.canceled -= OnSecondaryCanceled;
        secondaryAction.action.Disable();
    }

    void OnPrimaryPerformed(InputAction.CallbackContext ctx)
    {
        isPrimaryPressed = true;
        if (!castCooldownAllow) return;
        if (CurrentCastDuration == 0) TryShoot();
        else if (!isSecondaryPressed)
        {
            ClearCasting();
            castingRoutine = StartCoroutine(SpellCasting());
        }
    }

    void OnPrimaryCanceled(InputAction.CallbackContext ctx)
    {
        isPrimaryPressed = false;
    }

    void OnSecondaryPerformed(InputAction.CallbackContext ctx)
    {
        isSecondaryPressed = true;
    }

    void OnSecondaryCanceled(InputAction.CallbackContext ctx)
    {
        isSecondaryPressed = false;
    }

    public bool CooldownReady => castCooldownAllow;

    public void BeginCooldown(float seconds) => spellCooldown.StartCooldown(seconds);

    void TryShoot()
    {

        if (!castCooldownAllow) return;
        if (ammo.ammo >= CurrentMagickaCost) SuccesfulShoot(); else ShootFail();
    }

    void ShootFail()
    {
        handAnimator.SetTrigger("Hand_Fail");
        noManaLightAnimation.LightAnimation();
        spellcastInstance = RuntimeManager.CreateInstance(shootFailEvent);
        spellcastInstance.start();
        spellcastInstance.release();
    }

    void SuccesfulShoot()
    {
        handAnimator.SetTrigger("Spell_Shot_Quick");
        playCastBlast.StopCastingParticles(selectedBulletIndex);
        playCastBlast.Play(selectedBulletIndex);
        castCooldownTime = currentBullet.myCooldown * PlayerClasses.SpellCooldownMultiplier;
        ammo.ammo -= CurrentMagickaCost;
        ammoDisplay.Refresh(false);
        ammo.MagickaChanged();
        spellCooldown.StartCooldown(castCooldownTime);
        spellProjectileSpawner.SpawnSpell(selectedBulletIndex);
        SendShakeSignalShoot();
    }

    public bool LockToSpell(int spellIndex)
    {
        if (bulletPrefab == null || spellIndex < 0 || spellIndex >= bulletPrefab.Length || !bulletPrefab[spellIndex])
        {
            Debug.LogError($"{nameof(Attack)}: no spell prefab at index {spellIndex} to lock to - keeping the normal inventory.", this);
            return false;
        }

        SelectSpell(spellIndex);
        return true;
    }

    public void SelectSpell(int selectedSpell)
    {
        selectedBulletIndex = selectedSpell;
        SetCurrentBullet(selectedBulletIndex);
        changeSelectedSpell.Invoke();
        pullupEventInstance = RuntimeManager.CreateInstance(currentBullet.pullupSound);
        pullupEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        pullupEventInstance.start();
    }

    IEnumerator SpellCasting()
    {
        if (ammo.ammo >= CurrentMagickaCost)
        {
            spellCooldown.myCanvas.enabled = true;
            playCastBlast.StartCastingParticles(selectedBulletIndex);
            PlayerMovement.Instance.SlowMeDown();
            handAnimator.SetTrigger("Spell_Casting");
            SendShakeSignalCastStart();
            float castDuration = CurrentCastDuration;
            while (castingProgress < castDuration)
            {
                if (!isPrimaryPressed || isSecondaryPressed)
                {
                    handAnimator.SetTrigger("Spell_Casting_Stop");
                    ClearCasting();
                    yield break;
                }
                castingProgress += Time.deltaTime;
                castingSlider.value = castingProgress / castDuration;
                yield return null;
            }
            castingCompleted.Invoke();
            while (isPrimaryPressed)
            {
                if (isSecondaryPressed)
                {
                    handAnimator.SetTrigger("Spell_Casting_Stop");
                    ClearCasting();
                    yield break;
                }
                yield return null;
            }
            ClearCasting();
            TryShoot();
        }
        else ShootFail();
    }

    void ClearCasting()
    {
        PlayerMovement.Instance.SpeedMeBackUp();
        handAnimator.ResetTrigger("Spell_Casting");
        spellCooldown.myCanvas.enabled = false;
        castingSlider.value = 0;
        castingProgress = 0;
        playCastBlast.StopCastingParticles(selectedBulletIndex);
        spellCooldown.DisableComponents();
        CancelCasting.Invoke();
        if (castingRoutine != null) StopCoroutine(castingRoutine);
    }

    void SendShakeSignalShoot()
    {
        if (currentShake) currentShake.PlayShoot();
    }

    void SendShakeSignalCastStart()
    {
        if (currentShake) currentShake.PlayCastStart();
    }
}
