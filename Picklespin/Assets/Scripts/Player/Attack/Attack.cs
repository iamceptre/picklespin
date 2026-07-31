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
    // read live, so a casting-time wish applies to the spell already in hand
    float CurrentCastDuration => currentBullet ? currentBullet.castDuration * WishUpgrades.CastDurationMultiplier : 0f;
    int CurrentMagickaCost => currentBullet
        ? Mathf.Max(1, Mathf.RoundToInt(currentBullet.magickaCost * WishUpgrades.MagickaCostMultiplier))
        : 0;
    [SerializeField] Slider castingSlider;
    [SerializeField] UnityEvent castingCompleted;
    [SerializeField] UnityEvent CancelCasting;
    public Bullet currentBullet;
    CameraShakeManagerV2 camShakeManager;
    [SerializeField] InputActionReference primaryAction;
    [SerializeField] InputActionReference secondaryAction;
    bool isPrimaryPressed;
    bool isSecondaryPressed;
    Coroutine castingRoutine;

    void Awake()
    {
        currentBullet = bulletPrefab[selectedBulletIndex];
        if (instance != null && instance != this) Destroy(this); else instance = this;
    }

    void Start()
    {
        handAnimator = PublicPlayerHandAnimator.instance._animator;
        ammo = Ammo.instance;
        playCastBlast = PlayCastBlast.instance;
        ammoDisplay = AmmoDisplay.instance;
        spellProjectileSpawner = SpellProjectileSpawner.instance;
        camShakeManager = CameraShakeManagerV2.instance;
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

    // the cooldown bar is shared with the dash: neither chains into the other
    public bool CooldownReady => castCooldownAllow;

    public void BeginCooldown(float seconds) => spellCooldown.StartCooldown(seconds);

    void TryShoot()
    {
        // a cast held through a dash waits its turn; nothing has been spent yet
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
        playCastBlast.StopCastingParticles(selectedBulletIndex); // no-ops for a spell with no casting VFX
        playCastBlast.Play(selectedBulletIndex);
        castCooldownTime = currentBullet.myCooldown * PlayerClasses.SpellCooldownMultiplier;
        ammo.ammo -= CurrentMagickaCost;
        ammoDisplay.Refresh(false);
        ammo.MagickaChanged();
        spellCooldown.StartCooldown(castCooldownTime);
        spellProjectileSpawner.SpawnSpell(selectedBulletIndex);
        SendShakeSignalShoot(selectedBulletIndex);
    }

    // false = empty prefab slot, and the caller leaves the inventory alone
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
        currentBullet = bulletPrefab[selectedBulletIndex];
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
            SendShakeSignalCastStart(selectedBulletIndex);
            float castDuration = CurrentCastDuration; // fixed for this cast
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

    void SendShakeSignalShoot(int index)
    {
        switch (index)
        {
            case 0:
                camShakeManager.ShakeSelected(4);
                camShakeManager.ShakeHand(0.3f, 0.2f, 30);
                break;
            case 1:
                camShakeManager.ShakeSelected(5);
                camShakeManager.ShakeHand(0.4f, 0.2f, 15);
                break;
        }
    }

    void SendShakeSignalCastStart(int index)
    {
        switch (index)
        {
            case 1:
                camShakeManager.ShakeSelected(7);
                break;
        }
    }
}
