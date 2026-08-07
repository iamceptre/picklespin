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
    public SpellId selectedSpell;
    public float castCooldownTime = 0.1f;
    public bool castCooldownAllow = true;
    [SerializeField] UnlockedSpells unlockedSpells;
    [SerializeField] SpellCooldown spellCooldown;
    [SerializeField] NoManaLightAnimation noManaLightAnimation;
    public float castingProgress = 0;
    public bool IsCasting { get; private set; }

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
    Coroutine castingRoutine;

    bool PrimaryHeld => primaryAction.action.IsPressed();
    bool SecondaryHeld => secondaryAction.action.IsPressed();

    void Awake()
    {
        SetCurrentBullet(selectedSpell);
        if (instance != null && instance != this) Destroy(this); else instance = this;
    }

    void SetCurrentBullet(SpellId spell)
    {
        currentBullet = bulletPrefab[(int)spell];
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
        primaryAction.action.Enable();
        secondaryAction.action.Enable();
    }

    void OnDisable()
    {
        IsCasting = false;
        primaryAction.action.performed -= OnPrimaryPerformed;
        primaryAction.action.Disable();
    }

    void OnPrimaryPerformed(InputAction.CallbackContext ctx)
    {
        if (!castCooldownAllow) return;
        if (CurrentCastDuration == 0) TryShoot();
        else if (!SecondaryHeld)
        {
            ClearCasting();
            castingRoutine = StartCoroutine(SpellCasting());
        }
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
        playCastBlast.StopCastingParticles(selectedSpell);
        playCastBlast.Play(selectedSpell);
        castCooldownTime = currentBullet.myCooldown * PlayerClasses.SpellCooldownMultiplier;
        ammo.ammo -= CurrentMagickaCost;
        ammoDisplay.Refresh(false);
        ammo.MagickaChanged();
        spellCooldown.StartCooldown(castCooldownTime);
        spellProjectileSpawner.SpawnSpell(selectedSpell);
        SendShakeSignalShoot();
    }

    public bool LockToSpell(SpellId spell)
    {
        int slot = (int)spell;
        if (bulletPrefab == null || slot < 0 || slot >= bulletPrefab.Length || !bulletPrefab[slot])
        {
            DevLog.Error($"{nameof(Attack)}: no spell prefab for {spell} to lock to - keeping the normal inventory.", this);
            return false;
        }

        return SelectSpell(spell);
    }

    public bool SelectSpell(SpellId spell)
    {
        int slot = (int)spell;
        if (bulletPrefab == null || slot < 0 || slot >= bulletPrefab.Length || !bulletPrefab[slot])
        {
            DevLog.Error($"{nameof(Attack)}: no spell prefab for {spell} - ignoring selection.", this);
            return false;
        }

        selectedSpell = spell;
        SetCurrentBullet(selectedSpell);
        changeSelectedSpell.Invoke();
        pullupEventInstance = RuntimeManager.CreateInstance(currentBullet.pullupSound);
        pullupEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        pullupEventInstance.start();
        return true;
    }

    IEnumerator SpellCasting()
    {
        if (ammo.ammo >= CurrentMagickaCost)
        {
            IsCasting = true;
            spellCooldown.myCanvas.enabled = true;
            playCastBlast.StartCastingParticles(selectedSpell);
            PlayerMovement.Instance.SlowMeDown();
            handAnimator.SetTrigger("Spell_Casting");
            SendShakeSignalCastStart();
            float castDuration = CurrentCastDuration;
            while (castingProgress < castDuration)
            {
                if (!PrimaryHeld || SecondaryHeld)
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
            while (PrimaryHeld)
            {
                if (SecondaryHeld)
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
        IsCasting = false;
        PlayerMovement.Instance.SpeedMeBackUp();
        handAnimator.ResetTrigger("Spell_Casting");
        spellCooldown.myCanvas.enabled = false;
        castingSlider.value = 0;
        castingProgress = 0;
        playCastBlast.StopCastingParticles(selectedSpell);
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
